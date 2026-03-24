using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Interface
{
    public interface ITicketRepository
    {
        public void CreateTicket(Ticket ticket);

        public void GetTicketsByCustomerId(string customerId);

        public void GetOpenTicketsBy(string customerId);
    }
}
