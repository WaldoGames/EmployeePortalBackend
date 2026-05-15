using EmployeePortalBackend.Enums;
using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.DTO.ìmageDtos
{
    public class UploadRequestActiveDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public UploadStatus status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ValidUntilDate { get; set; }
        public string ObjectKey { get; set; }
        public Customer Customer { get; set; }
    }
}
