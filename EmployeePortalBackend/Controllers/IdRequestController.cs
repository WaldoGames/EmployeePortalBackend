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

            // Validate actual file content via magic bytes (do not trust extension or content-type)
            if (!await IsValidImageAsync(file))
                return BadRequest("File is not a valid image. Only JPEG, PNG, GIF, WebP, and BMP are accepted.");

            _logger.LogInformation("Id request for request {Request} is attempting to be completed", id);

            using (var stream = file.OpenReadStream())
            {
                bool success = await imageRequestService.UploadImageAsync(stream, id, file.ContentType, _vaultOptions.NormalKey);
                if (success)
                {
                    return Ok(new { message = "Upload successful" });
                }
            }

            _logger.LogError("Id request for request {Request} failed", id);
            return StatusCode(500, "Internal server error during upload");
        }

        private static async Task<bool> IsValidImageAsync(IFormFile file)
        {
            var imageSignatures = new Dictionary<string, byte[][]>
            {
                { "JPEG",  [ [0xFF, 0xD8, 0xFF] ] },
                { "PNG",   [ [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] ] },
                { "GIF",   [ [0x47, 0x49, 0x46, 0x38, 0x37, 0x61],   
                             [0x47, 0x49, 0x46, 0x38, 0x39, 0x61] ] }, 
                { "WebP",  [ [0x52, 0x49, 0x46, 0x46] ] },             
                { "BMP",   [ [0x42, 0x4D] ] },
            };
            const int headerSize = 12;
            byte[] header = new byte[headerSize];

            using var stream = file.OpenReadStream();
            int bytesRead = await stream.ReadAsync(header, 0, headerSize);

            if (bytesRead < 4) return false;

            foreach (var (format, signatures) in imageSignatures)
            {
                foreach (var sig in signatures)
                {
                    if (bytesRead >= sig.Length && header.Take(sig.Length).SequenceEqual(sig))
                    {
                        if (format == "WebP")
                        {
                            if (bytesRead < 12) return false;
                            byte[] webpMark = [0x57, 0x45, 0x42, 0x50];
                            return header.Skip(8).Take(4).SequenceEqual(webpMark);
                        }

                        return true;
                    }
                }
            }

            return false;
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
