using EmployeePortalBackend.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace EmployeePortalBackend.Interface
{
    public interface IBasicCustomerRepository
    {
        public Customer? TryGetCustomerById(string id);

        public Task PostCustomer(Customer customer);

        public List<Customer> TrySearchByFirstName(string firstName);

        public List<TrigramHashes> TryGetTrigramHashesByCustomerId(string customerId);
        public List<TrigramHashes> TryGetTrigramsHashesByHash(List<string> name);
        public List<Customer> TrySearchForCustomers(List<string> hashes);

        public List<string> CheckForNewTrigrams(List<string> trigramhashes);

        public Task PostTrigramHashes(List<string> trigramHashes);

        public void ConnectUserToTrigramHashes(string customerId, List<string> trigramHashIds);

        public void DeleteCustomer(string customerId);
    }
}
