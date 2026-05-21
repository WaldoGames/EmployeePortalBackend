using EmployeePortalBackend.DTO.ìmageDtos;
using EmployeePortalBackend.Services;
using EmployeePortalBackend.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Npgsql;
using Microsoft.AspNetCore.Authorization;

namespace EmployeePortalBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdRequestController : Controller
    {
        private CustomerService customerService;
        private ImageRequestService imageRequestService;

        private readonly ILogger<TicketController> _logger;
        private readonly VaultKeySettings _vaultOptions;

        public IdRequestController(CustomerService customerService, ImageRequestService imageRequestService, ILogger<TicketController> logger, IOptions<VaultKeySettings> vaultOptions)
        {
            this.customerService = customerService;
            this.imageRequestService = imageRequestService;
            _logger = logger;
            _vaultOptions = vaultOptions.Value;
        }
        
        [Authorize(Roles = "RequestId")]
        [HttpPost]
        public async Task<IActionResult> GenerateImageRequest(CreateUploadRequestDto requestDto)
        {
            _logger.LogInformation("Id request for user {CustomerId} made by {UserName}", requestDto.CustomerId ,User.FindFirstValue(ClaimTypes.NameIdentifier));
            string? url = await imageRequestService.GenerateInitialUploadUrl(requestDto, getKey());

            if (url == null)
            {
                _logger.LogError("Failed to generate upload URL. for customer {customer} attempted by {employee}", requestDto.CustomerId, User.FindFirstValue(ClaimTypes.NameIdentifier));

                return BadRequest("Failed to generate upload URL.");
            }

            return Ok(new { UploadUrl = url });

        }

        [HttpPost("upload-direct")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string id)
        {

            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            _logger.LogInformation("Id request for request {Request} is attempting to be completed", id);
            using (var stream = file.OpenReadStream())
            {
                // We use the file name or a generated GUID
                bool success = await imageRequestService.UploadImageAsync(stream, id, file.ContentType, _vaultOptions.NormalKey);//note: if enough time find a way to remove the key from here. but image uploads have no jwt token so a normal key is provided by default for now

                if (success)
                {
                    // Since the backend did the work, it already knows it's successful!
                    // You can update your database status here directly.
                    return Ok(new { message = "Upload successful" });
                }
            }
            _logger.LogError("Id request for request {Request} failed", id);
            return StatusCode(500, "Internal server error during upload");
        }
        [Authorize(Roles = "SensitiveDataAccess")]
        [HttpGet("load-direct")]
        public async Task<IActionResult> LoadImage([FromQuery] string customerId)
        {
            var (stream, contentType, expired) = await imageRequestService.GetImageAsync(customerId, getKey());

            if (expired)
            {
                _logger.LogError("{user} attempted to load an users Id for customer: {customerId} but it failed reason: expired", User.FindFirstValue(ClaimTypes.NameIdentifier), customerId);
                return NotFound(new { message = "Image has expired" });
            }

            if (stream == null)
            {
                _logger.LogError("{user} attempted to load an users Id for customer: {customerId} but it failed reason: unknown", User.FindFirstValue(ClaimTypes.NameIdentifier), customerId);

                return NotFound();
            }


            _logger.LogInformation("{user} attempted to load an users Id for customer: {customerId}", User.FindFirstValue(ClaimTypes.NameIdentifier), customerId);
            return File(stream, contentType ?? "application/octet-stream");
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
