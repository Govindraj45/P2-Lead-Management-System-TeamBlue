// Bring in the tools we need for database access and user models
using LeadManagementSystem.Data;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

// This tells C# which folder/group this file belongs to
namespace LeadManagementSystem.Auth;

// This class creates the initial test users in the database when the app starts for the first time
public class SeedDataService
{
    // _db lets us read from and write to the database
    private readonly LeadDbContext _db;
    // _logger writes messages so we can see what happened
    private readonly ILogger<SeedDataService> _logger;

    // Constructor — stores the database and logger tools for later use
    public SeedDataService(LeadDbContext db, ILogger<SeedDataService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // This method adds starter users to the database (only if none exist yet)
    public async Task SeedUsersAsync()
    {
        // Count how many users are already in the database
        var existingCount = await _db.Users.CountAsync();
        // If there are already users, skip seeding — we don't want duplicates
        if (existingCount > 0)
        {
            _logger.LogInformation("Users already seeded ({Count} found), skipping", existingCount);
            return;
        }

        // Create a list of three test users — one for each role in the system
        var users = new List<User>
        {
            // Admin user — has full control over everything
            new()
            {
                Email = "admin@leadcrm.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Admin",
                Role = "Admin"
            },
            // Sales Manager — can manage leads and oversee sales reps
            new()
            {
                Email = "manager@leadcrm.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                FullName = "Sales Manager",
                Role = "SalesManager"
            },
            // Sales Rep — works with leads directly
            new()
            {
                Email = "rep@leadcrm.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Rep@123"),
                FullName = "Sales Rep",
                Role = "SalesRep"
            }
        };

        // Add all three users to the database at once
        _db.Users.AddRange(users);
        // Save the changes to the database
        await _db.SaveChangesAsync();
        // Log how many users were created
        _logger.LogInformation("Seeded {Count} test users", users.Count);
    }
}

/*
 * FILE SUMMARY: SeedDataService.cs
 * This file creates the initial set of test users when the application starts for the very first time.
 * It adds three users: an Admin, a Sales Manager, and a Sales Rep — each with a different role.
 * The passwords are securely hashed using BCrypt before being saved to the database.
 * If users already exist in the database, it skips the process to avoid creating duplicates.
 * This is essential for development and testing so you can immediately log in and use the app.
 */
