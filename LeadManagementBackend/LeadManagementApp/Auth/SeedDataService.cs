using LeadManagementSystem.Data;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Auth;

public class SeedDataService
{
    private readonly LeadDbContext _db;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(LeadDbContext db, ILogger<SeedDataService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedUsersAsync()
    {
        var existingCount = await _db.Users.CountAsync();
        if (existingCount > 0)
        {
            _logger.LogInformation("Users already seeded ({Count} found), skipping", existingCount);
            return;
        }

        var users = new List<User>
        {
            new()
            {
                Email = "admin@leadcrm.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Admin",
                Role = "Admin"
            },
            new()
            {
                Email = "manager@leadcrm.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                FullName = "Sales Manager",
                Role = "SalesManager"
            },
            new()
            {
                Email = "rep@leadcrm.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Rep@123"),
                FullName = "Sales Rep",
                Role = "SalesRep"
            }
        };

        _db.Users.AddRange(users);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} test users", users.Count);
    }
}
