namespace PowerLens.Api.Domain;

public sealed class Componente
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public Projeto? Projeto { get; set; }
    public Guid? ParentComponentId { get; set; }
    public Componente? ParentComponent { get; set; }
    public ComponentType Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NomeLogico { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Documentacao? Documentacao { get; set; }
    public List<Componente> Children { get; set; } = [];
}
