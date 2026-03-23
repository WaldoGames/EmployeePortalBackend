using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.Model
{
    public class Ticket
    {
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public string CreatedDate { get; set; }
        public string? EditedDate { get; set; }

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string CreatorEmployeeId { get; set; }
        public string CreatorEmployeeName { get; set; }

    }
}
