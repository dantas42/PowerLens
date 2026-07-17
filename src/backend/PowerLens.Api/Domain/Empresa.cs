namespace PowerLens.Api.Domain;

public sealed class Empresa
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Projeto> Projetos { get; set; } = [];
}
