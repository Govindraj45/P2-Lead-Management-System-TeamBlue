// This tells C# which folder/group this file belongs to
namespace LeadManagementSystem.Auth;

// This class holds all the settings needed to create and verify JWT tokens
// (JWT = JSON Web Token — a secure way to prove a user is logged in)
public class JwtSettings
{
    // This is the name used to find these settings in the app's configuration file (appsettings.json)
    public const string SectionName = "Jwt";

    // The secret key used to sign tokens — like a stamp that proves the token is real
    public string Secret { get; set; } = null!;
    // The "issuer" is who created the token (our app's name)
    public string Issuer { get; set; } = "LeadManagementSystem";
    // The "audience" is who the token is meant for (also our app)
    public string Audience { get; set; } = "LeadManagementSystem";
    // How many minutes the token stays valid before the user must log in again
    public int ExpirationMinutes { get; set; } = 60;
}

/*
 * FILE SUMMARY: JwtSettings.cs
 * This file defines the configuration settings for JWT (JSON Web Token) authentication.
 * It stores the secret key, issuer name, audience name, and token expiration time.
 * These values are read from the appsettings.json config file when the app starts up.
 * Without these settings, the app wouldn't know how to create or verify login tokens.
 */
