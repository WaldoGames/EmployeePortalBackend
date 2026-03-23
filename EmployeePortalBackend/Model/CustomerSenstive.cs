using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.Model
{
    public class CustomerSenstive
    {
        [Key]
        public int Id { get; set; }
        public string Adress;
        public string PhoneNumber;

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
        //public ??? Id TO DO FIGUREOUT HOW TO HANDEL ID.
    }
}
