using EmployeePortalBackend.Context;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;
using EmployeePortalBackend.Repository;
using EmployeePortalBackend.Services;
using EmployeePortalBackend.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token (e.g., Bearer <token>)",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });


    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.Configure<VaultOptions>(opts =>
{
    opts.AgentAddress = Environment.GetEnvironmentVariable("VAULT_AGENT_ADDRESS")
                        ?? "";

    var token = Environment.GetEnvironmentVariable("Vault_Token")
                    ?? "";

    opts.AgentToken = token;
});
builder.Services.AddSingleton<VaultService>();

builder.Services.Configure<VaultKeySettings>(
    builder.Configuration.GetSection("Vault")
);

builder.Services.Configure<VaultKeySettings>(
    builder.Configuration.GetSection("Vault")
);
builder.Services.Configure<MiniOSettings>(
    builder.Configuration.GetSection("MiniO")
);
//MiniO

//logger
#region logger

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "timestamp",  new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    { "level",      new LevelColumnWriter(renderAsText: true, dbType: NpgsqlDbType.Varchar) },
    { "message",    new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "exception",  new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "source_ctx", new SinglePropertyColumnWriter("SourceContext", format: "l", dbType: NpgsqlDbType.Varchar) },
    { "machine",    new SinglePropertyColumnWriter("MachineName",   format: "l", dbType: NpgsqlDbType.Varchar) },
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("LogWriter"),
        tableName: "logs",
        columnOptions: columnWriters,
        needAutoCreateTable: false,   // table already exists from init.sql
        batchSizeLimit: 50,
        period: TimeSpan.FromSeconds(5),
        failureCallback: ex =>
            {
                Console.Error.WriteLine(builder.Configuration.GetConnectionString("LogWriter"));

                // This fires when a batch fails to write to Postgres
                Console.Error.WriteLine($"[LOG SINK FAILURE] {ex.Message}");
                    // or alert via email, Slack, health check endpoint, etc.
            }
    )
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

builder.Services.AddScoped<IBasicCustomerRepository, BasicCustomerRepository>();
builder.Services.AddScoped<CustomerService>();

builder.Services.AddScoped<ITicketRepository, TickerRepository>();
builder.Services.AddScoped<TicketService>();

builder.Services.AddScoped<IIdRequestRepository, IdRequestRepository>();
builder.Services.AddScoped<ImageRequestService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:*")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});
builder.Services.AddDbContext<BasicCustomerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Customer_db")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {

        options.RequireHttpsMetadata = false;

        options.Authority = "https://keycloak:7443/realms/Employee";

        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuers = new[]
            {
                "https://10.10.10.103:7443/realms/Employee",
                "https://keycloak:7443/realms/Employee"
            },
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context => {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity == null) return Task.CompletedTask;

                // Extract realm_access.roles from the token
                var realmAccess = context.Principal?
                    .FindFirst("realm_access")?.Value;

                if (realmAccess != null)
                {
                    var parsed = JsonDocument.Parse(realmAccess);
                    if (parsed.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };

    });


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

