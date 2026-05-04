using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.Model
{
    public class Customer
    {
        [Key]
        public string Id { get; set; }//not encrypted

        public string FirstNameHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string email { get; set; }

        public CustomerSenstive? CustomerSenstive { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<IdRequest> IdRequests { get; set; }

        public List<TrigramHashes> TrigramHashes { get; set; }
    }
}
