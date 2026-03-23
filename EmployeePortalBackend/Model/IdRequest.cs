using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.Model
{
    public class IdRequest
    {
        [Key]
        public string Id { get; set; }
        public string CustomerId { get; set; }

        public string EmployeeId { get; set; }

        public string EmployeeName { get; set; }
        public string link { get; set; }
        public string status { get; set; }

        public string CreatedDate { get; set; }

        public string ValidUntilDate { get; set; }

        public Customer Customer { get; set; }


    }
}
