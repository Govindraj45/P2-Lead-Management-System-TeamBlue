using LeadManagementSystem.Data;
using LeadManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadManagementSystem.Auth;

public class AuthService
{
    private readonly LeadDbContext _db;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(LeadDbContext db, TokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for {Email}", email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for {Email}", email);
            return null;
        }

        _logger.LogInformation("User {Email} logged in successfully with role {Role}", email, user.Role);
        return _tokenService.GenerateToken(user);
    }
}
