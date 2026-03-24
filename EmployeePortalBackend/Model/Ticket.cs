using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeePortalBackend.Model
{
    public class Ticket
    {
        [Key]
        public string Id { get; set; }//not encrypted on an application level
        public string Title { get; set; }//not encrypted on an application level
        public string Description { get; set; }
        public string Status { get; set; }//not encrypted on an application level

        public string CreatedDate { get; set; }//not encrypted on an application level
        public string? EditedDate { get; set; }//not encrypted on an application level

        public string CustomerId { get; set; }//not encrypted on an application level
        public Customer Customer { get; set; }//not encrypted on an application level
        public string CreatorEmployeeId { get; set; }//not encrypted on an application level
        public string CreatorEmployeeName { get; set; }//not encrypted on an application level

    }
}
