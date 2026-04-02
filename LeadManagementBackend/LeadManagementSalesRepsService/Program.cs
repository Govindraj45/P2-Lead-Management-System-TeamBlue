using Consul;
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.SalesReps;
using LeadManagementSystem.Interfaces;
using MediatR;
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

builder.Services.AddMediatR(typeof(CreateSalesRepCommand).Assembly);
builder.Services.AddControllers();

builder.Services.AddScoped<ISalesRepository, EfSalesRepository>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementSalesRepsService",
    status = "running",
    database = "SQL Server"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementSalesRepsService",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
