using EmployeePortalBackend.Context;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Repository
{
    public class IdRequestRepository : IIdRequestRepository
    {
        BasicCustomerContext customerContext;

        public IdRequestRepository(BasicCustomerContext customerContext)
        {
            this.customerContext = customerContext;
        }

        public void CreateIdRequest(IdRequest request)
        {
            customerContext.IdRequests.Add(request);
            customerContext.SaveChanges();
        }

        public bool DoesActiveRequestExistForUser(string userId)
        {
            bool x = customerContext.IdRequests.OrderByDescending(r => r.CreatedDate).Any(r => r.CustomerId == userId && r.status == "pending");

            return x;
        }

        public IdRequest? GetIdRequestForUser(string userId)
        {
            return customerContext.IdRequests.Where(r => r.CustomerId == userId).OrderByDescending(r => r.CreatedDate).FirstOrDefault();
        }

        public IdRequest? TryGetIdRequestConflictingWithNewRequesr(string userId)
        {
            throw new NotImplementedException();
        }

        public IdRequest? TryGetObjectKeyForId(string Id)
        {
            return customerContext.IdRequests.Where(r => r.Id == Id).OrderByDescending(r => r.CreatedDate).FirstOrDefault();
        }

        public void UpdateIdRequest(IdRequest request)
        {
            customerContext.SaveChanges();
        }
    }
}
