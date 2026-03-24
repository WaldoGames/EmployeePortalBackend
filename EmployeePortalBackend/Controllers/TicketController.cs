using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.DTO.TicketsDtos;
using EmployeePortalBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePortalBackend.Controllers
{
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
            string id = Guid.NewGuid().ToString();

            await ticketService.createTicket(test);


            return Ok(id);
        }
        //[HttpGet("posttest/{id}")]
    }
}
