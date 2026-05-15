using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.DTO.TicketsDtos;
using EmployeePortalBackend.Logger;
using EmployeePortalBackend.Model;
using EmployeePortalBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        public TicketController(CustomerService customerService, TicketService ticketService, ILogger<TicketController> logger)
        {
            this.customerService = customerService;
            this.ticketService = ticketService;
            _logger = logger;
        }

        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] CreateTicketDto test)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string id = Guid.NewGuid().ToString();

            var c = await ticketService.createTicket(test, id);

            if (c == null)
            {
                return BadRequest("Customer not found");
            }

            return Ok(id);
        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetTicketById(string Id)
        {
            DecryptedTicketDto? decryptedTicket = await ticketService.getTicketById(Id);

            if(decryptedTicket == null)
            {
                return BadRequest("Ticket not found");
            }
            return Ok(decryptedTicket);
        }

        [HttpGet("overviewlist")]
        public async Task<IActionResult> getTicketList()
        {

            List<GetTicketsBasicDto> tickets = await ticketService.GetTickets();
            return Ok(tickets);
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateTicket(string Id, [FromBody] EditTicketDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await ticketService.EditTicket(dto, Id);

            if (result == null)
            {
                return BadRequest("Ticket not found");
            }

            return Ok(new { message = result });
        }

        //[HttpGet("posttest/{id}")]
    }
}
