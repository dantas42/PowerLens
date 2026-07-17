namespace PowerLens.Api.Domain;

public sealed class Historico
{
    public Guid Id { get; set; }
    public Guid ComponenteId { get; set; }
    public Componente? Componente { get; set; }
    public string? Commit { get; set; }
    public DateTimeOffset Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
