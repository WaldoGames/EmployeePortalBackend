using EmployeePortalBackend.DTO.ìmageDtos;
using EmployeePortalBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePortalBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdRequestController : Controller
    {
        private CustomerService customerService;
        private ImageRequestService imageRequestService;

        private readonly ILogger<TicketController> _logger;

        public IdRequestController(CustomerService customerService, ImageRequestService imageRequestService, ILogger<TicketController> logger)
        {
            this.customerService = customerService;
            this.imageRequestService = imageRequestService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateImageRequest(CreateUploadRequestDto requestDto)
        {

            string url = await imageRequestService.GenerateInitialUploadUrl(requestDto);

            if (url == null)
            {
                return BadRequest("Failed to generate upload URL.");
            }

            return Ok(new { UploadUrl = url });

        }

        [HttpPost("upload-direct")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string id)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            using (var stream = file.OpenReadStream())
            {
                // We use the file name or a generated GUID
                bool success = await imageRequestService.UploadImageAsync(stream, id, file.ContentType);

                if (success)
                {
                    // Since the backend did the work, it already knows it's successful!
                    // You can update your database status here directly.
                    return Ok(new { message = "Upload successful" });
                }
            }

            return StatusCode(500, "Internal server error during upload");
        }
        [HttpGet("load-direct")]
        public async Task<IActionResult> LoadImage([FromQuery] string customerId)
        {
            var (stream, contentType, expired) = await imageRequestService.GetImageAsync(customerId);

            if (expired)
                return NotFound(new { message = "Image has expired" });

            if (stream == null)
                return NotFound();

            return File(stream, contentType ?? "application/octet-stream");
        }

    }
}
