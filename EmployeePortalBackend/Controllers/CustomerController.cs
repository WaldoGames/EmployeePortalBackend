using EmployeePortalBackend.DTO;
using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePortalBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController: Controller
    {
        private CustomerService customerService;

        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
        {
            this.customerService = customerService;
            _logger = logger;
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
    }
}
