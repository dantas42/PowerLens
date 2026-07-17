using PowerLens.Api.Domain;

namespace PowerLens.Api.Contracts;

public sealed record RegisterRequest(string Nome, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, UserResponse User);
public sealed record UserResponse(string Id, string Nome, string Email);

public sealed record CreateEmpresaRequest(string Nome);
public sealed record EmpresaResponse(Guid Id, string Nome);

public sealed record CreateProjetoRequest(string Nome, string? DataverseUrl);
public sealed record ProjetoResponse(Guid Id, Guid EmpresaId, string Nome, string? DataverseUrl, bool HasDataverseConnection);

public sealed record UpsertDataverseConnectionRequest(string DataverseUrl, string TenantId, string ClientId, string ClientSecret);
public sealed record DataverseConnectionResponse(string? DataverseUrl, string? TenantId, string? ClientId, bool HasClientSecret);

public sealed record ComponentResponse(
    Guid Id,
    Guid ProjetoId,
    Guid? ParentComponentId,
    ComponentType Tipo,
    string Nome,
    string NomeLogico,
    string? Descricao,
    bool IsDocumentationGenerated);

public sealed record ComponentDetailsResponse(
    ComponentResponse Component,
    IReadOnlyList<ComponentResponse> Fields,
    IReadOnlyList<RelationshipResponse> Relationships,
    IReadOnlyList<HistoryResponse> History);

public sealed record DocumentationResponse(Guid Id, Guid ComponenteId, string ConteudoMarkdown, bool IsGenerated, DateTimeOffset UpdatedAt);
public sealed record UpdateDocumentationRequest(string ConteudoMarkdown);

public sealed record RelationshipResponse(Guid Id, Guid OrigemComponenteId, Guid DestinoComponenteId, RelationshipType TipoRelacionamento, string DestinoNome, string DestinoNomeLogico);
public sealed record HistoryResponse(Guid Id, string? Commit, DateTimeOffset Data, string Descricao);
public sealed record SyncResultResponse(int Entities, int Fields, int Choices, int Relationships);
public sealed record LinkTargetResponse(Guid Id, string Nome, string NomeLogico, ComponentType Tipo);
