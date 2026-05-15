using EmployeePortalBackend.Model;
using System.ComponentModel.DataAnnotations;

namespace EmployeePortalBackend.DTO.ìmageDtos
{
    public class CreateUploadRequestDto
    {
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
