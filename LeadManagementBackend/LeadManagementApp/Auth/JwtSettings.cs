namespace LeadManagementSystem.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = "LeadManagementSystem";
    public string Audience { get; set; } = "LeadManagementSystem";
    public int ExpirationMinutes { get; set; } = 60;
}
