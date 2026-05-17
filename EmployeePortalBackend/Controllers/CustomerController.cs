using EmployeePortalBackend.DTO;
using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Services;
using EmployeePortalBackend.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Security.Claims;

namespace EmployeePortalBackend.Controllers
{
    [Authorize(Roles = "EmployeeEditUsers")]
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private CustomerService customerService;

        private readonly ILogger<CustomerController> _logger;
        private VaultService vc;
        private readonly VaultKeySettings _vaultOptions;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger, VaultService vc, IOptions<VaultKeySettings> vaultOptions)
        {
            this.customerService = customerService;
            _logger = logger;
            this.vc = vc;
            _vaultOptions = vaultOptions.Value;
        }
        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] NewCustomerDto test)
        {
            _logger.LogInformation("Received new customer, created by {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier) );
            string id = Guid.NewGuid().ToString();

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
            _logger.LogInformation("Requested basic customer information, requested by {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return await customerService.DecryptedBasicCustomerobject(id, getKey());
        }
        [HttpGet("search/{promt}")]
        public async Task<ActionResult<List<SearchResultDto>>> Search(string promt)
        {
            _logger.LogInformation("Search request {promt} done by {UserId}", promt, User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<SearchResultDto> result = await customerService.searchUsers(promt, getKey());
            return Ok(result);
        }

        private string getKey()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return string.Empty;
            }

            return User.IsInRole("SensitiveInformation")
                ? _vaultOptions.SensitiveKey
                : _vaultOptions.NormalKey;
        }
    }
}
