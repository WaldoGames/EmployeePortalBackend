using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Services
{
    public class CustomerService
    {
        IBasicCustomerRepository _repository;
        encryptionService encryption;

        public CustomerService(IBasicCustomerRepository repo)
        {
            _repository = repo;
            encryption = new encryptionService();
        }

        public async Task NewUser(CustomerInternalDto newCustomer, string key)
        {
            Customer customer = await encryption.EncryptCustomerAsync(newCustomer, key);

            _repository.PostCustomer(customer);
        }

        public async Task<DecryptedBasicCustomerobject?> DecryptedBasicCustomerobject(string id, string key)
        {
            Customer c = _repository.TryGetCustomerById(id);

            if (c == null)
            {
                return null;
            }

            return await encryption.DecrypteCustomer(c, key);
        }

        public async Task<List<SearchResultDto>> searchUsers(string promt)
        {
            var hash = await encryption.EncryptSha(promt, "donutdonutgodonuts");

            List<Customer> results = _repository.TrySearchByFirstName(hash);

            List<SearchResultDto> resultDtos = new List<SearchResultDto>();

            foreach (var item in results)
            {
                DecryptedBasicCustomerobject customer = await encryption.DecrypteCustomer(item, "kek-standard");

                resultDtos.Add(new SearchResultDto
                {
                    fullName = (customer.FirstName + " " + customer.LastName),
                    id = customer.Id
                });
            }
            return resultDtos;
        }
    }
}
