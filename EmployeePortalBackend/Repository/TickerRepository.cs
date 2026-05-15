using EmployeePortalBackend.Context;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Repository
{
    public class TickerRepository : ITicketRepository
    {
        BasicCustomerContext customerContext;

        public TickerRepository(BasicCustomerContext customerContext)
        {
            this.customerContext = customerContext;
        }

        public void CreateTicket(Ticket ticket)
        {
            customerContext.Tickets.Add(ticket);
            customerContext.SaveChanges();
        }

        public Ticket? GetOpenTicketsById(string Id)
        {
            return customerContext.Tickets.Where(t => t.Id == Id).FirstOrDefault();
        }

        public List<Ticket> GetTickets(int limit, int offset)
        {
            return customerContext.Tickets.OrderByDescending(x => x.Status == "Open").ThenBy(x => x.EditedDate).Skip(offset * limit).Take(limit).ToList();
        }

        public List<Ticket> GetTicketsByCustomerId(string customerId)
        {
            return customerContext.Tickets.Where(t => t.CustomerId == customerId).ToList();
        }

        public void UpdateTicket()
        {
            customerContext.SaveChanges();
        }
    }
}
