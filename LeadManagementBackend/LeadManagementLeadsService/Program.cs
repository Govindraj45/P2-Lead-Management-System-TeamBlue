using Consul;
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LeadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));

builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
builder.Services.AddHostedService<ConsulRegistrationHostedService>();

// CQRS Handlers (simple service-based, no MediatR)
builder.Services.AddScoped<CreateLeadHandler>();
builder.Services.AddScoped<GetAllLeadsHandler>();
builder.Services.AddScoped<GetLeadByIdHandler>();
builder.Services.AddScoped<UpdateLeadHandler>();
builder.Services.AddScoped<UpdateLeadStatusHandler>();
builder.Services.AddScoped<DeleteLeadHandler>();
builder.Services.AddScoped<ConvertLeadToCustomerHandler>();
builder.Services.AddControllers();

builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();
builder.Services.AddScoped<LeadService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementLeadsService",
    status = "running",
    database = "SQL Server"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementLeadsService",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
