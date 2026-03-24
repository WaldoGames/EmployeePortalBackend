using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.DTO.TicketsDtos
{
    public class CreateTicketDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CustomerId { get; set; }
        public string CreatorEmployeeId { get; set; }
        public string CreatorEmployeeName { get; set; }
    }
}
