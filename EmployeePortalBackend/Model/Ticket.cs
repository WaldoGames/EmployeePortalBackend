namespace EmployeePortalBackend.Model
{
    public class Ticket
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
            public DateTime? EditedDate { get; set; }

        public string CustomerId { get; set; }
        public string CreatorEmployeeId { get; set; }
        public string CreatorEmployeeName { get; set; }

    }
}
