namespace PowerLens.Api.Domain;

public sealed class Relacionamento
{
    public Guid Id { get; set; }
    public Guid OrigemComponenteId { get; set; }
    public Componente? OrigemComponente { get; set; }
    public Guid DestinoComponenteId { get; set; }
    public Componente? DestinoComponente { get; set; }
    public RelationshipType TipoRelacionamento { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
