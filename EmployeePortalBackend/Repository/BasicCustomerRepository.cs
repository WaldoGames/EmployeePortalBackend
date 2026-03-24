using EmployeePortalBackend.Context;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Repository
{
    public class BasicCustomerRepository : IBasicCustomerRepository
    {
        BasicCustomerContext customerContext;

        public BasicCustomerRepository(BasicCustomerContext customerContext)
        {
            this.customerContext = customerContext;
        }
        public void PostCustomer(Customer customer)
        {
            customerContext.Customers.Add(customer);
            customerContext.SaveChanges();
        }

        public Customer? TryGetCustomerById(string id)
        {
            return customerContext.Customers.Where(c=>c.Id == id).FirstOrDefault();     
        }

        public List<Customer> TrySearchByFirstName(string firstNameHashed)
        {
            return customerContext.Customers.Where(c => c.FirstNameHash == firstNameHashed).ToList();
        }
    }
}
