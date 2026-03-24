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

        public void GetOpenTicketsBy(string customerId)
        {
            throw new NotImplementedException();
        }

        public void GetTicketsByCustomerId(string customerId)
        {
            throw new NotImplementedException();
        }
    }
}
