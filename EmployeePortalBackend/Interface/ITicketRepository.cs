using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Interface
{
    public interface ITicketRepository
    {
        public void CreateTicket(Ticket ticket);

        public List<Ticket> GetTicketsByCustomerId(string customerId);

        public Ticket? GetOpenTicketsById(string Id);

        public List<Ticket> GetTickets(int limit, int offset);
    }
}
