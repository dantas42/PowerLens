namespace PowerLens.Api.Domain;

public sealed class Projeto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? DataverseUrl { get; set; }
    public string? DataverseTenantId { get; set; }
    public string? DataverseClientId { get; set; }
    public string? DataverseClientSecretProtected { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<Componente> Componentes { get; set; } = [];
}
