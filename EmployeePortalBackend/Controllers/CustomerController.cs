using EmployeePortalBackend.DTO;
using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePortalBackend.Controllers
{
    //[Authorize(Roles = "EmployeeEditUsers")]
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private CustomerService customerService;

        private readonly ILogger<CustomerController> _logger;
        private VaultService vc;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger, VaultService vc)
        {
            this.customerService = customerService;
            _logger = logger;
            this.vc = vc;
        }
        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] NewCustomerDto test)
        {
            string id = Guid.NewGuid().ToString();

            //only first field is used during this test. the rest is just some default values:
            await customerService.NewUser(new CustomerInternalDto
            {
                FirstName = test.FirstName,
                LastName = test.LastName,
                Birthday = test.Birthday.ToString(),
                Email = test.Email,
                Id = id
            }, "kek-standard");
            return Ok(id);
        }
        [HttpGet("posttest/{id}")]
        public async Task<DecryptedBasicCustomerobject?> Get(string id)
        {
            return await customerService.DecryptedBasicCustomerobject(id, "kek-standard");
        }
        [HttpGet("search/{promt}")]
        public async Task<ActionResult<List<SearchResultDto>>> Search(string promt)
        {
            //TODO implement fuzzy blind indexing so i can search without the full name being needed
            List<SearchResultDto> result = await customerService.searchUsers(promt);
            return Ok(result);
        }

        [HttpGet("trigramtest/{promt}")]
        public async Task<ActionResult<List<string>>> TrigramTest(string promt)
        {
            string[] trigrams = customerService.generateTrigrams(promt);
            return Ok(trigrams);
        }
        [HttpGet("HashTest/{promt}")]
        public async Task<ActionResult<string>> test(string promt)
        {

            string[] trigrams = customerService.generateTrigrams(promt);

            return Ok(await vc.ComputeHmacBatchAsync(trigrams));
        }
        
    }
}
