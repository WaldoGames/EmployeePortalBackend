using EmployeePortalBackend.DTO;
using EmployeePortalBackend.Model;
using EmployeePortalBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePortalBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : Controller
    {
        private CustomerService customerService;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(CustomerService customerService, ILogger<WeatherForecastController> logger)
        {
            this.customerService = customerService;
            _logger = logger;
        }




        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpPost("posttest/{test}/{id}")]
        public async Task<IActionResult> Post(string test, string id)
        {

            //only first field is used during this test. the rest is just some default values:
            await customerService.NewUser(new NewCustomerDto
            {
                FirstName = test,
                LastName = "last name",
                Birthday = "01/01/1990",
                Id = id
            }, "kek-standard");
            return Ok();
        }
        [HttpGet("posttest/{id}")]
        public async Task<DecryptedBasicCustomerobject?> Get(string id)
        {
            return await customerService.DecryptedBasicCustomerobject(id, "kek-standard");
        }
    }
}
