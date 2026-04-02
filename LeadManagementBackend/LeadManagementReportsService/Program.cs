using Consul;
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.Reports;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

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

// Redis Distributed Cache
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "LMS_Reports_";
});

builder.Services.AddMediatR(typeof(GetLeadStatusDistributionQuery).Assembly);
builder.Services.AddControllers();

builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementReportsService",
    status = "running",
    database = "SQL Server"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementReportsService",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
