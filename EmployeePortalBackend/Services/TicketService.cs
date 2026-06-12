using EmployeePortalBackend.DTO.TicketsDtos;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;

namespace EmployeePortalBackend.Services
{
    public class TicketService
    {
        IBasicCustomerRepository _customerRepository;
        ITicketRepository _repository;
        VaultService encryption;

        public TicketService(IBasicCustomerRepository repo, ITicketRepository trepo, VaultService vc)
        {
            _customerRepository = repo;
            _repository = trepo;
            encryption = vc;
        }

        public async Task<string?> createTicket(CreateTicketDto dto, string id, string key, string employeeId, string employeeName)
        {
            var customer = _customerRepository.TryGetCustomerById(dto.CustomerId);
            if (customer == null)
            {
                return null;
            }
            string encryptedDescription = await encryption.EncryptField(key, dto.Description);

            Ticket newTicket = new Ticket
            {
                Id = id,
                Title = dto.Title,
                Description = encryptedDescription,
                Status = "Open",
                CreatedDate = DateTime.UtcNow.ToString(),
                EditedDate = DateTime.UtcNow.ToString(),
                CustomerId = dto.CustomerId,
                CreatorEmployeeId = employeeId,
                CreatorEmployeeName = employeeName,

            };
            _repository.CreateTicket(newTicket);
            return id;
        }

        public async Task<DecryptedTicketDto?> getTicketById(string ticketId, string key)
        {
            Ticket? t = _repository.GetOpenTicketsById(ticketId);
            if(t == null)
            {
                return null;
            }

            DecryptedTicketDto ticket = await DecryptTicket(t, key);

            return ticket;
        }
        public async Task<List<GetTicketsBasicDto>> GetTickets(int limit = 50, int offset = 0)
        {
            List<Ticket> tickets= _repository.GetTickets(limit, offset);

            List<GetTicketsBasicDto> OverviewTickets = new List<GetTicketsBasicDto>();

            foreach (var item in tickets)
            {
                OverviewTickets.Add(new GetTicketsBasicDto
                {
                    EditedDate = item.EditedDate,
                    Id = item.Id,
                    Status = item.Status,
                    Title = item.Title
                });
            }
            return OverviewTickets;
        }

        public async Task<string> EditTicket(EditTicketDTO newData, string ticketId, string key)
        {
            Ticket? existingTicket = _repository.GetOpenTicketsById(ticketId);
            if (existingTicket == null)
            {
                return null;
            }

            existingTicket.Title = newData.Title;
            existingTicket.Description = await encryption.EncryptField(key, newData.Description);
            existingTicket.Status = newData.Status;
            existingTicket.EditedDate = DateTime.UtcNow.ToString();

            _repository.UpdateTicket();
            return ticketId;
        }

        public async Task<DecryptedTicketDto> DecryptTicket(Ticket ticket, string key)
        {
            string decryptedDescription = await encryption.DecryptField(key, ticket.Description);

            DecryptedTicketDto decryptedTicketDto = new DecryptedTicketDto()
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = decryptedDescription,
                Status = ticket.Status,
                CreatedDate = ticket.CreatedDate,
                EditedDate = ticket.EditedDate,
                CustomerId = ticket.CustomerId,
                CreatorEmployeeId = ticket.CreatorEmployeeId,
                CreatorEmployeeName = ticket.CreatorEmployeeName,
            };

            return decryptedTicketDto;
        }
        public async Task<Ticket> EncryptTicket(DecryptedTicketDto decryptedTicketDto, string key)
        {

            string encryptedDescription = await encryption.EncryptField(key, decryptedTicketDto.Description);

            Ticket ticket = new Ticket()
            {
                Id = decryptedTicketDto.Id,
                Title = decryptedTicketDto.Title,
                Description = encryptedDescription,
                Status = decryptedTicketDto.Status,
                CreatedDate = decryptedTicketDto.CreatedDate,
                EditedDate = decryptedTicketDto.EditedDate,
                CustomerId = decryptedTicketDto.CustomerId,
                CreatorEmployeeId = decryptedTicketDto.CreatorEmployeeId,
                CreatorEmployeeName = decryptedTicketDto.CreatorEmployeeName,
            };

            return ticket;

        }
    }
}
