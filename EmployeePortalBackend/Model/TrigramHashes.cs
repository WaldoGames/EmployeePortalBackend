using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.Model
{
    public class TrigramHashes
    {
        [Key]
        public string id { get; set; }

        public string FullnamePartHash { get; set; }

        public List<Customer> Customers { get; set; }
    }
}
