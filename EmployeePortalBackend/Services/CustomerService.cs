using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;
using System.Security.Cryptography.X509Certificates;

namespace EmployeePortalBackend.Services
{
    public class CustomerService
    {
        IBasicCustomerRepository _repository;
        VaultService encryption;

        public CustomerService(IBasicCustomerRepository repo, VaultService vc)
        {
            _repository = repo;
            encryption = vc;
        }

        public async Task NewUser(CustomerInternalDto newCustomer, string key)
        {
            Customer customer = await encryption.EncryptCustomerAsync(newCustomer, key);

            await _repository.PostCustomer(customer);

            await ConnectUserToTrigrams(newCustomer.FullName, customer.Id, key);
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

        public async Task<List<SearchResultDto>> searchUsers(string promt, string key)
        {
            List<string> trigrams = generateTrigrams(promt).ToList();

            List<string> hashes = await encryption.ComputeHmacBatchAsync(trigrams, key);

            List<Customer> results = _repository.TrySearchForCustomers(hashes);

            List<SearchResultDto> resultDtos = new List<SearchResultDto>();

            foreach (var item in results)
            {
                DecryptedBasicCustomerobject customer = await encryption.DecrypteCustomer(item, key);

                resultDtos.Add(new SearchResultDto
                {
                    fullName = (customer.FirstName + " " + customer.LastName),
                    id = customer.Id
                });
            }
            return resultDtos;
        }

        public async Task<List<string>> GenerateTrigramHashes(string input)
        {
            string[] trigrams = generateTrigrams(input);
            List<string> trigramHashes = new List<string>();
            foreach (var item in trigrams)
            {
                var hash = await encryption.ComputeHmacAsync(item);
                trigramHashes.Add(hash);
            }
            return trigramHashes;
        }
        public async Task ConnectUserToTrigrams(string Fullname, string customerId, string key)
        {
            List<string> trigrams = generateTrigrams(Fullname).ToList();
            List<string> trigramHashes = await encryption.ComputeHmacBatchAsync(trigrams, key);
            List<string> newHashes = _repository.CheckForNewTrigrams(trigramHashes);
            if (newHashes.Count > 0)
            {
                await _repository.PostTrigramHashes(newHashes);
            }

            List<TrigramHashes> hashIds = _repository.TryGetTrigramsHashesByHash(trigramHashes);
            _repository.ConnectUserToTrigramHashes(customerId, hashIds.Select(t=>t.id).ToList());
        }
        public string[] generateTrigrams(string input)
        {
            input = input.ToLower();

            string[] trigrams = new string[input.Length + 2];
            for (int i = -2; i < input.Length; i++)
            {
                string a = i < 0 ? " " : input[i].ToString();
                string b = (i + 1) < 0 || (i + 1) >= input.Length ? " " : input[i + 1].ToString();
                string c = (i + 2) >= input.Length ? " " : input[i + 2].ToString();

                trigrams[i + 2] = a + b + c;
            }
            return trigrams;
        }

    }
}
