using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.DTO.TicketsDtos
{
    public class EditTicketDTO
    {
        public string? Title { get; set; }//not encrypted on an application level
        public string? Description { get; set; }
        public string? Status { get; set; }//not encrypted on an application level
    }
}
