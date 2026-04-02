// These "using" lines bring in tools we need from other parts of the project
using LeadManagementSystem.Data;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

// This tells C# which folder/group this file belongs to
namespace LeadManagementSystem.Auth;

// This class handles user login — it checks email and password, then returns a token
public class AuthService
{
    // These are private tools this class needs to do its job
    // _db lets us talk to the database
    private readonly LeadDbContext _db;
    // _tokenService creates login tokens (like a digital ID badge)
    private readonly TokenService _tokenService;
    // _logger writes messages to a log so we can see what happened
    private readonly ILogger<AuthService> _logger;

    // This is the constructor — it runs when the class is created and stores the tools we need
    public AuthService(LeadDbContext db, TokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    // This method tries to log a user in with their email and password
    // It returns a token string if successful, or null if login fails
    public async Task<string?> LoginAsync(string email, string password)
    {
        // Look up the user in the database by their email address
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        // If no user was found with that email, log a warning and return nothing
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for {Email}", email);
            return null;
        }

        // Check if the password they typed matches the stored password hash
        // (We never store plain passwords — we store a scrambled version called a "hash")
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for {Email}", email);
            return null;
        }

        // If we get here, login was successful! Log it and create a token for the user
        _logger.LogInformation("User {Email} logged in successfully with role {Role}", email, user.Role);
        return _tokenService.GenerateToken(user);
    }
}

/*
 * FILE SUMMARY: AuthService.cs
 * This file is the login handler for the application. When a user tries to sign in,
 * this service looks up their email in the database, checks if the password is correct,
 * and if everything matches, creates a JWT token (like a temporary ID card) for them.
 * If the email doesn't exist or the password is wrong, it returns nothing and logs a warning.
 * This is one of the most important files for security because it controls who can access the system.
 */
