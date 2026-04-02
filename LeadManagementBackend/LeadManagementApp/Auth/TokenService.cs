// These "using" lines bring in the tools we need for creating JWT tokens
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeadManagementSystem.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// This tells C# which folder/group this file belongs to
namespace LeadManagementSystem.Auth;

// This class is responsible for creating JWT tokens — the digital ID badges users carry after logging in
public class TokenService
{
    // _settings stores the JWT configuration (secret key, expiration time, etc.)
    private readonly JwtSettings _settings;

    // Constructor — reads the JWT settings from the app's configuration
    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    // This method builds a JWT token for a specific user after they successfully log in
    public string GenerateToken(User user)
    {
        // Turn the secret key string into a security key that can sign the token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        // Set up the signing method — HmacSha256 is a secure algorithm for signing
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // "Claims" are pieces of information stored inside the token
        // Think of them like fields on an ID card (name, role, email, etc.)
        var claims = new List<Claim>
        {
            new("UserId", user.UserId.ToString()),
            new("Email", user.Email),
            new("Role", user.Role),
            // ClaimTypes.Role is the standard way .NET checks roles for authorization
            new(ClaimTypes.Role, user.Role),
            // "Sub" (subject) is a standard JWT field for the user's ID
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            // "Jti" is a unique ID for this specific token (prevents token reuse attacks)
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // If the user is a SalesRep, add their SalesRepId so the app knows which rep they are
        if (user.Role == "SalesRep")
        {
            claims.Add(new Claim("SalesRepId", user.UserId.ToString()));
        }

        // Build the actual JWT token with all the pieces: issuer, audience, claims, expiration, and signature
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        // Convert the token object into a string that can be sent to the user
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/*
 * FILE SUMMARY: TokenService.cs
 * This file creates JWT (JSON Web Token) tokens for users after they log in successfully.
 * A JWT token is like a digital ID badge — it contains the user's ID, email, and role.
 * The token is signed with a secret key so the server can verify it's real and hasn't been tampered with.
 * Each token expires after a set time (default 60 minutes), forcing the user to log in again.
 * This is a core security file — every authenticated API request uses the token created here.
 */
