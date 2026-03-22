using EmployeePortalBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace EmployeePortalBackend.Context
{
    public class BasicCustomerContext: DbContext
    {
        public BasicCustomerContext(DbContextOptions<BasicCustomerContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerSenstive> CustomerSenstives { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<IdRequest> IdRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer -> CustomerSensitive (1-to-1)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.CustomerSenstive)
                .WithOne(cs => cs.Customer)
                .HasForeignKey<CustomerSenstive>(cs => cs.CustomerId)
                .IsRequired();

            // Customer -> Tickets (1-to-many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Tickets)
                .WithOne(t => t.Customer)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer -> IdRequests (1-to-many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.IdRequests)
                .WithOne(ir => ir.Customer)
                .HasForeignKey(ir => ir.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
