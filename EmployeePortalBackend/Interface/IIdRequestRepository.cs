using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Interface
{
    public interface IIdRequestRepository
    {
        public void CreateIdRequest(Model.IdRequest request);

        public void UpdateIdRequest(Model.IdRequest request);

        public Model.IdRequest? GetIdRequestForUser(string userId);
        public Model.IdRequest? TryGetIdRequestConflictingWithNewRequesr(string userId);
        public bool DoesActiveRequestExistForUser(string userId);

        public IdRequest? TryGetObjectKeyForId(string Id);

    }
}
