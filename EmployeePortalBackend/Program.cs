using EmployeePortalBackend.Context;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;
using EmployeePortalBackend.Repository;
using EmployeePortalBackend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBasicCustomerRepository, BasicCustomerRepository>();
builder.Services.AddScoped<CustomerService>();

builder.Services.AddScoped<ITicketRepository, TickerRepository>();
builder.Services.AddScoped<TicketService>();


builder.Services.AddDbContext<BasicCustomerContext>(options =>
    options.UseNpgsql("Host=customer-db;Port=5432;Database=mydb;Username=myuser;Password=mypassword;Include Error Detail=true;"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
