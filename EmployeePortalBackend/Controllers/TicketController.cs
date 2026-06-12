using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.DTO.TicketsDtos;
using EmployeePortalBackend.Logger;
using EmployeePortalBackend.Model;
using EmployeePortalBackend.Services;
using EmployeePortalBackend.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EmployeePortalBackend.Controllers
{
    [Authorize(Roles = "EmployeeTickets")]
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private CustomerService customerService;
        private TicketService ticketService;

        private readonly ILogger<TicketController> _logger;
        private readonly VaultKeySettings _vaultOptions;


        public TicketController(CustomerService customerService, TicketService ticketService, ILogger<TicketController> logger, IOptions<VaultKeySettings> vaultOptions)
        {
            this.customerService = customerService;
            this.ticketService = ticketService;
            _logger = logger;
            _vaultOptions = vaultOptions.Value;
        }

        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] CreateTicketDto test)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("unable to create ticket, attempted by employee:{name}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(ModelState);
            }

            string id = Guid.NewGuid().ToString();

            //var x = User;

            var c = await ticketService.createTicket(test, id, getKey(), User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue("preferred_username"));

            if (c == null)
            {
                _logger.LogError("unable to create ticket by {name}",User.FindFirstValue(ClaimTypes.NameIdentifier));

                return BadRequest("Customer not found");
            }
            _logger.LogInformation("new ticket created by: {employee}", User.FindFirstValue(ClaimTypes.NameIdentifier));


            return Ok(id);
        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetTicketById(string Id)
        {
            DecryptedTicketDto? decryptedTicket = await ticketService.getTicketById(Id, getKey());

            if(decryptedTicket == null)
            {
                _logger.LogError("unable to load ticket {id}", Id);
                return BadRequest("Ticket not found");
            }
            _logger.LogInformation("ticket {id} loaded by: {employee}", Id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(decryptedTicket);
        }

        [HttpGet("overviewlist")]
        public async Task<IActionResult> getTicketList()
        {
            _logger.LogInformation("ticket list loaded by: {employee}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<GetTicketsBasicDto> tickets = await ticketService.GetTickets();
            return Ok(tickets);
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateTicket(string Id, [FromBody] EditTicketDTO dto)
        {
            

            if (!ModelState.IsValid)
            {
                _logger.LogError("unable to update ticket {id} model state invalid", Id);
                return BadRequest(ModelState);
            }

            var result = await ticketService.EditTicket(dto, Id, getKey());

            if (result == null)
            {
                _logger.LogError("unable to update ticket {id}", Id);
                return BadRequest("Ticket not found");
            }
            _logger.LogInformation("ticket {id} updated by: {employee}", Id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(new { message = result });
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

        //[HttpGet("posttest/{id}")]
    }
}
