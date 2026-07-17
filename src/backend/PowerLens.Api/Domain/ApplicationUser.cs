using Microsoft.AspNetCore.Identity;

namespace PowerLens.Api.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;
}
