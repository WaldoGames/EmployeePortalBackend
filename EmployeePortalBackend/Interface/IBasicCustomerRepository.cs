using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Interface
{
    public interface IBasicCustomerRepository
    {
        public Customer? TryGetCustomerById(string id);

        public void PostCustomer(Customer customer);

        public List<Customer> TrySearchByFirstName(string firstName);
    }
}
