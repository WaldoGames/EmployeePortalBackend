using EmployeePortalBackend.Context;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace EmployeePortalBackend.Repository
{
    public class BasicCustomerRepository : IBasicCustomerRepository
    {
        BasicCustomerContext customerContext;

        public BasicCustomerRepository(BasicCustomerContext customerContext)
        {
            this.customerContext = customerContext;
        }

        public List<string> CheckForNewTrigrams(List<string> trigramhashes)
        {
            List<string> Hashes = customerContext.TrigramHashes.Where(t => trigramhashes.Contains(t.FullnamePartHash)).Select(s=>s.FullnamePartHash).ToList();

            List<string> newHashes = new List<string>();

            foreach (string item in trigramhashes)
            {
                if (!Hashes.Contains(item))
                {
                    newHashes.Add(item);
                }
            }
            return newHashes;
        }

        public void ConnectUserToTrigramHashes(string customerId, List<string> trigramHashIds)
        {
            Customer customer = customerContext.Customers.Include(c=>c.TrigramHashes).Where(c => c.Id == customerId).FirstOrDefault();

            if(customer != null)
            {
                List<TrigramHashes> trigramHashes = customerContext.TrigramHashes.Where(t => trigramHashIds.Contains(t.id)).ToList();
                customer.TrigramHashes.AddRange(trigramHashes);
                customerContext.SaveChanges();
            }
            else
            {
                throw new Exception("Customer not found");
            }
        }

        public async Task PostCustomer(Customer customer)
        {
            customerContext.Customers.Add(customer);
            customerContext.SaveChanges();
        }

        public async Task PostTrigramHashes(List<string> trigramHashes)
        {
            customerContext.TrigramHashes.AddRange(trigramHashes.Select(h => new TrigramHashes { id = Guid.NewGuid().ToString(), FullnamePartHash = h }));
            await customerContext.SaveChangesAsync();
        }

        public Customer? TryGetCustomerById(string id)
        {
            return customerContext.Customers.Where(c=>c.Id == id).FirstOrDefault();     
        }

        public List<TrigramHashes> TryGetTrigramHashesByCustomerId(string customerId)
        {
            List<TrigramHashes> trigramHashes = customerContext.TrigramHashes.Where(t => t.Customers.Any(c => c.Id == customerId)).ToList();
            return trigramHashes;
        }

        public List<TrigramHashes> TryGetTrigramsHashesByHash(List<string> name)
        {
            List<TrigramHashes> trigramHashes = customerContext.TrigramHashes.Where(t => name.Contains(t.FullnamePartHash)).ToList();
            return trigramHashes;
        }

        public List<Customer> TrySearchForCustomers(List<string> hashes)
        {
            int searchHashCount = hashes.Count;

            //List<Customer> matchedCustomersr = customerContext.Customers.Include(c => c.TrigramHashes).ToList();

            List<Customer> matchedCustomers = customerContext.Customers
                // 1. PERFORMANCE: If this is just for display, use AsNoTracking to save memory
                .AsNoTracking()

                // 2. Filter: Keep the Any() so we don't calculate math on completely irrelevant rows
                .Where(c => c.TrigramHashes.Any(th => hashes.Contains(th.FullnamePartHash)))

                .Select(c => new
                {
                    Customer = c,
                    MatchCount = c.TrigramHashes.Count(th => hashes.Contains(th.FullnamePartHash)),
                    TotalCount = c.TrigramHashes.Count()
                })

                // 3. OPTIMIZED SORT: Use the Sørensen–Dice coefficient
                .OrderByDescending(x => (2.0 * x.MatchCount) / (searchHashCount + x.TotalCount))

                .Select(x => x.Customer)

                // 4. PERFORMANCE: Always cap searches to prevent massive memory spikes
                .Take(50)
                .ToList();

            return matchedCustomers;
        }

        public List<Customer> TrySearchByFirstName(string firstNameHashed)
        {
            return customerContext.Customers.Where(c => c.FirstNameHash == firstNameHashed).ToList();
        }

        public void DeleteCustomer(string customerId)
        {
            Customer customer = customerContext.Customers.Where(c => c.Id == customerId).FirstOrDefault();

            if (customer != null)
            {
                customerContext.Customers.Remove(customer);
                customerContext.SaveChanges();
            }
        }
    }
}
