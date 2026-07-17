namespace PowerLens.Api.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "PowerLens";
    public string Audience { get; set; } = "PowerLens";
    public string SigningKey { get; set; } = "PowerLens local development signing key. Replace in production.";
    public int ExpirationMinutes { get; set; } = 480;
}
