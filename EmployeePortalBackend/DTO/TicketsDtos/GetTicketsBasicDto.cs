using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.DTO.TicketsDtos
{
    public class GetTicketsBasicDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string EditedDate { get; set; }
    }
}
