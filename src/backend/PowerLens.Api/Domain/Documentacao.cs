namespace PowerLens.Api.Domain;

public sealed class Documentacao
{
    public Guid Id { get; set; }
    public Guid ComponenteId { get; set; }
    public Componente? Componente { get; set; }
    public string ConteudoMarkdown { get; set; } = string.Empty;
    public bool IsGenerated { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UltimoAutorId { get; set; }
}
