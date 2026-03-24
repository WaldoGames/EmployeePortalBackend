using EmployeePortalBackend.DTO.TicketsDtos;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;

namespace EmployeePortalBackend.Services
{
    public class TicketService
    {
        IBasicCustomerRepository _customerRepository;
        ITicketRepository _repository;
        encryptionService encryption;

        public TicketService(IBasicCustomerRepository repo, ITicketRepository trepo)
        {
            _customerRepository = repo;
            _repository = trepo;
            encryption = new encryptionService();
        }

        public async Task<string?> createTicket(CreateTicketDto dto)
        {
            var customer = _customerRepository.TryGetCustomerById(dto.CustomerId);
            if (customer == null)
            {
                return null;
            }

            string id = Guid.NewGuid().ToString();
            string encryptedDescription = await encryption.EncryptField(dto.Description, "kek-standard");

            Ticket newTicket = new Ticket
            {
                Id = id,
                Title = dto.Title,
                Description = encryptedDescription,
                Status = "Open",
                CreatedDate = DateTime.UtcNow.ToString(),
                EditedDate = DateTime.UtcNow.ToString(),
                CustomerId = dto.CustomerId,
                CreatorEmployeeId = dto.CreatorEmployeeId,
                CreatorEmployeeName = dto.CreatorEmployeeName,

            };
            _repository.CreateTicket(newTicket);
            return id;
        }
    }
}
