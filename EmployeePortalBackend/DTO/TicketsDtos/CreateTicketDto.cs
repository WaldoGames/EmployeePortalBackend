using EmployeePortalBackend.Model;
using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.DTO.TicketsDtos
{
    public class CreateTicketDto
    {
        [Required(ErrorMessage ="title is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "title must be between 1 and 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "description is required")]
        [StringLength(1000, ErrorMessage = "description must be less than 1000 characters")]
        public string Description { get; set; }

        [StringLength(36, MinimumLength =36, ErrorMessage = "customerId not a valid id")]
        public string CustomerId { get; set; }
        [StringLength(36, MinimumLength = 36, ErrorMessage = "CreatorId not a valid id")]
        public string CreatorEmployeeId { get; set; }

        [StringLength(100, ErrorMessage = "Creator name should be less then 100 characters")]
        public string CreatorEmployeeName { get; set; }
    }
}
