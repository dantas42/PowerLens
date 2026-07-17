using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PowerLens.Api.Contracts;
using PowerLens.Api.Data;
using PowerLens.Api.Domain;
using PowerLens.Api.Services;

namespace PowerLens.Api.Endpoints;

public static class EndpointMapping
{
    public static IEndpointRouteBuilder MapPowerLensEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapPost("/auth/register", RegisterAsync);
        api.MapPost("/auth/login", LoginAsync);
        api.MapGet("/auth/me", MeAsync).RequireAuthorization();

        api.MapGet("/empresas", GetEmpresasAsync).RequireAuthorization();
        api.MapPost("/empresas", CreateEmpresaAsync).RequireAuthorization();
        api.MapDelete("/empresas/{empresaId:guid}", DeleteEmpresaAsync).RequireAuthorization();
        api.MapGet("/empresas/{empresaId:guid}/projetos", GetProjetosAsync).RequireAuthorization();
        api.MapPost("/empresas/{empresaId:guid}/projetos", CreateProjetoAsync).RequireAuthorization();

        api.MapDelete("/projetos/{projetoId:guid}", DeleteProjetoAsync).RequireAuthorization();
        api.MapGet("/projetos/{projetoId:guid}/dataverse-connection", GetDataverseConnectionAsync).RequireAuthorization();
        api.MapPut("/projetos/{projetoId:guid}/dataverse-connection", UpdateDataverseConnectionAsync).RequireAuthorization();
        api.MapPost("/projetos/{projetoId:guid}/sync/dataverse", SyncDataverseAsync).RequireAuthorization();
        api.MapGet("/projetos/{projetoId:guid}/componentes", GetComponentesAsync).RequireAuthorization();
        api.MapGet("/projetos/{projetoId:guid}/itens-sem-documentacao", GetUndocumentedAsync).RequireAuthorization();
        api.MapGet("/projetos/{projetoId:guid}/link-targets", GetLinkTargetsAsync).RequireAuthorization();

        api.MapGet("/componentes/{id:guid}", GetComponenteAsync).RequireAuthorization();
        api.MapDelete("/componentes/{id:guid}", DeleteComponenteAsync).RequireAuthorization();
        api.MapGet("/componentes/{id:guid}/documentacao", GetDocumentationAsync).RequireAuthorization();
        api.MapPut("/componentes/{id:guid}/documentacao", UpdateDocumentationAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, UserManager<ApplicationUser> users, JwtTokenService tokens)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Email e senha sao obrigatorios." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Nome = string.IsNullOrWhiteSpace(request.Nome) ? request.Email : request.Nome,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors.Select(x => x.Description) });
        }

        return Results.Ok(new AuthResponse(tokens.CreateToken(user), ToUser(user)));
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, JwtTokenService tokens)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        return result.Succeeded
            ? Results.Ok(new AuthResponse(tokens.CreateToken(user), ToUser(user)))
            : Results.Unauthorized();
    }

    private static async Task<IResult> MeAsync(ClaimsPrincipal principal, UserManager<ApplicationUser> users)
    {
        var user = await users.GetUserAsync(principal);
        return user is null ? Results.Unauthorized() : Results.Ok(ToUser(user));
    }

    private static async Task<IResult> GetEmpresasAsync(PowerLensDbContext db)
    {
        var empresas = await db.Empresas.OrderBy(x => x.Nome).Select(x => new EmpresaResponse(x.Id, x.Nome)).ToListAsync();
        return Results.Ok(empresas);
    }

    private static async Task<IResult> CreateEmpresaAsync(CreateEmpresaRequest request, PowerLensDbContext db)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Results.BadRequest(new { message = "Nome e obrigatorio." });
        }

        var empresa = new Empresa { Id = Guid.NewGuid(), Nome = request.Nome.Trim() };
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();

        return Results.Created($"/api/empresas/{empresa.Id}", new EmpresaResponse(empresa.Id, empresa.Nome));
    }

    private static async Task<IResult> DeleteEmpresaAsync(Guid empresaId, PowerLensDbContext db, CancellationToken cancellationToken)
    {
        var empresa = await db.Empresas.FindAsync([empresaId], cancellationToken);
        if (empresa is null)
        {
            return Results.NotFound();
        }

        var projetoIds = await db.Projetos
            .Where(x => x.EmpresaId == empresaId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var projetoId in projetoIds)
        {
            await DeleteProjetoGraphAsync(projetoId, db, cancellationToken);
        }

        db.Empresas.Remove(empresa);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetProjetosAsync(Guid empresaId, PowerLensDbContext db)
    {
        var projetos = await db.Projetos
            .Where(x => x.EmpresaId == empresaId)
            .OrderBy(x => x.Nome)
            .Select(x => ToProjeto(x))
            .ToListAsync();

        return Results.Ok(projetos);
    }

    private static async Task<IResult> CreateProjetoAsync(Guid empresaId, CreateProjetoRequest request, PowerLensDbContext db)
    {
        if (!await db.Empresas.AnyAsync(x => x.Id == empresaId))
        {
            return Results.NotFound();
        }

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nome = request.Nome.Trim(),
            DataverseUrl = string.IsNullOrWhiteSpace(request.DataverseUrl) ? null : request.DataverseUrl.Trim()
        };

        db.Projetos.Add(projeto);
        await db.SaveChangesAsync();

        return Results.Created($"/api/empresas/{empresaId}/projetos/{projeto.Id}", ToProjeto(projeto));
    }

    private static async Task<IResult> DeleteProjetoAsync(Guid projetoId, PowerLensDbContext db, CancellationToken cancellationToken)
    {
        var projeto = await db.Projetos.FindAsync([projetoId], cancellationToken);
        if (projeto is null)
        {
            return Results.NotFound();
        }

        await DeleteProjetoGraphAsync(projetoId, db, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetDataverseConnectionAsync(Guid projetoId, PowerLensDbContext db)
    {
        var projeto = await db.Projetos.FindAsync(projetoId);
        return projeto is null
            ? Results.NotFound()
            : Results.Ok(new DataverseConnectionResponse(projeto.DataverseUrl, projeto.DataverseTenantId, projeto.DataverseClientId, !string.IsNullOrWhiteSpace(projeto.DataverseClientSecretProtected)));
    }

    private static async Task<IResult> UpdateDataverseConnectionAsync(Guid projetoId, UpsertDataverseConnectionRequest request, PowerLensDbContext db, IDataProtectionProvider protectionProvider)
    {
        var projeto = await db.Projetos.FindAsync(projetoId);
        if (projeto is null)
        {
            return Results.NotFound();
        }

        var protector = protectionProvider.CreateProtector("PowerLens.Dataverse.ClientSecret");
        projeto.DataverseUrl = request.DataverseUrl.Trim();
        projeto.DataverseTenantId = request.TenantId.Trim();
        projeto.DataverseClientId = request.ClientId.Trim();
        projeto.DataverseClientSecretProtected = protector.Protect(request.ClientSecret);
        await db.SaveChangesAsync();

        return Results.Ok(new DataverseConnectionResponse(projeto.DataverseUrl, projeto.DataverseTenantId, projeto.DataverseClientId, true));
    }

    private static async Task<IResult> SyncDataverseAsync(Guid projetoId, DataverseSyncService sync, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sync.SyncAsync(projetoId, cancellationToken);
            return Results.Ok(new SyncResultResponse(result.Entities, result.Fields, result.Choices, result.Relationships));
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetComponentesAsync(Guid projetoId, ComponentType? tipo, string? search, PowerLensDbContext db)
    {
        var query = db.Componentes.Include(x => x.Documentacao).Where(x => x.ProjetoId == projetoId);

        if (tipo is not null)
        {
            query = query.Where(x => x.Tipo == tipo);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.Nome.ToLower().Contains(term) || x.NomeLogico.ToLower().Contains(term));
        }

        var components = await query.OrderBy(x => x.Tipo).ThenBy(x => x.Nome).Select(x => ToComponent(x)).ToListAsync();
        return Results.Ok(components);
    }

    private static async Task<IResult> GetUndocumentedAsync(Guid projetoId, PowerLensDbContext db)
    {
        var items = await db.Componentes
            .Include(x => x.Documentacao)
            .Where(x => x.ProjetoId == projetoId && (x.Documentacao == null || x.Documentacao.IsGenerated))
            .OrderBy(x => x.Tipo)
            .ThenBy(x => x.Nome)
            .Select(x => ToComponent(x))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> GetLinkTargetsAsync(Guid projetoId, PowerLensDbContext db)
    {
        var targets = await db.Componentes
            .Where(x => x.ProjetoId == projetoId)
            .OrderBy(x => x.Nome)
            .Select(x => new LinkTargetResponse(x.Id, x.Nome, x.NomeLogico, x.Tipo))
            .ToListAsync();

        return Results.Ok(targets);
    }

    private static async Task<IResult> GetComponenteAsync(Guid id, PowerLensDbContext db)
    {
        var component = await db.Componentes.Include(x => x.Documentacao).FirstOrDefaultAsync(x => x.Id == id);
        if (component is null)
        {
            return Results.NotFound();
        }

        var fields = await db.Componentes
            .Include(x => x.Documentacao)
            .Where(x => x.ParentComponentId == id)
            .OrderBy(x => x.Tipo)
            .ThenBy(x => x.Nome)
            .Select(x => ToComponent(x))
            .ToListAsync();

        var relationships = await db.Relacionamentos
            .Include(x => x.DestinoComponente)
            .Where(x => x.OrigemComponenteId == id)
            .OrderBy(x => x.TipoRelacionamento)
            .Select(x => new RelationshipResponse(x.Id, x.OrigemComponenteId, x.DestinoComponenteId, x.TipoRelacionamento, x.DestinoComponente!.Nome, x.DestinoComponente.NomeLogico))
            .ToListAsync();

        var history = await db.Historicos
            .Where(x => x.ComponenteId == id)
            .OrderByDescending(x => x.Data)
            .Select(x => new HistoryResponse(x.Id, x.Commit, x.Data, x.Descricao))
            .ToListAsync();

        return Results.Ok(new ComponentDetailsResponse(ToComponent(component), fields, relationships, history));
    }

    private static async Task<IResult> DeleteComponenteAsync(Guid id, PowerLensDbContext db, CancellationToken cancellationToken)
    {
        var component = await db.Componentes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (component is null)
        {
            return Results.NotFound();
        }

        var idsToDelete = await GetComponentTreeIdsAsync(component.ProjetoId, id, db, cancellationToken);

        await db.Relacionamentos
            .Where(x => idsToDelete.Contains(x.OrigemComponenteId) || idsToDelete.Contains(x.DestinoComponenteId))
            .ExecuteDeleteAsync(cancellationToken);
        await db.Documentacoes
            .Where(x => idsToDelete.Contains(x.ComponenteId))
            .ExecuteDeleteAsync(cancellationToken);
        await db.Historicos
            .Where(x => idsToDelete.Contains(x.ComponenteId))
            .ExecuteDeleteAsync(cancellationToken);
        await db.Componentes
            .Where(x => idsToDelete.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetDocumentationAsync(Guid id, PowerLensDbContext db)
    {
        var documentation = await db.Documentacoes.FirstOrDefaultAsync(x => x.ComponenteId == id);
        return documentation is null ? Results.NotFound() : Results.Ok(ToDocumentation(documentation));
    }

    private static async Task<IResult> UpdateDocumentationAsync(Guid id, UpdateDocumentationRequest request, ClaimsPrincipal principal, PowerLensDbContext db)
    {
        var documentation = await db.Documentacoes.FirstOrDefaultAsync(x => x.ComponenteId == id);
        if (documentation is null)
        {
            return Results.NotFound();
        }

        documentation.ConteudoMarkdown = request.ConteudoMarkdown;
        documentation.IsGenerated = false;
        documentation.UltimoAutorId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        await db.SaveChangesAsync();

        return Results.Ok(ToDocumentation(documentation));
    }

    private static UserResponse ToUser(ApplicationUser user) => new(user.Id.ToString(), user.Nome, user.Email ?? string.Empty);

    private static ProjetoResponse ToProjeto(Projeto projeto) =>
        new(projeto.Id, projeto.EmpresaId, projeto.Nome, projeto.DataverseUrl, !string.IsNullOrWhiteSpace(projeto.DataverseClientSecretProtected));

    private static ComponentResponse ToComponent(Componente component) =>
        new(component.Id, component.ProjetoId, component.ParentComponentId, component.Tipo, component.Nome, component.NomeLogico, component.Descricao, component.Documentacao?.IsGenerated ?? true);

    private static DocumentationResponse ToDocumentation(Documentacao documentation) =>
        new(documentation.Id, documentation.ComponenteId, documentation.ConteudoMarkdown, documentation.IsGenerated, documentation.UpdatedAt);

    private static async Task DeleteProjetoGraphAsync(Guid projetoId, PowerLensDbContext db, CancellationToken cancellationToken)
    {
        var componentIds = await db.Componentes
            .Where(x => x.ProjetoId == projetoId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (componentIds.Count > 0)
        {
            await db.Relacionamentos
                .Where(x => componentIds.Contains(x.OrigemComponenteId) || componentIds.Contains(x.DestinoComponenteId))
                .ExecuteDeleteAsync(cancellationToken);
            await db.Documentacoes
                .Where(x => componentIds.Contains(x.ComponenteId))
                .ExecuteDeleteAsync(cancellationToken);
            await db.Historicos
                .Where(x => componentIds.Contains(x.ComponenteId))
                .ExecuteDeleteAsync(cancellationToken);
            await db.Componentes
                .Where(x => componentIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await db.Projetos
            .Where(x => x.Id == projetoId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task<List<Guid>> GetComponentTreeIdsAsync(Guid projetoId, Guid rootId, PowerLensDbContext db, CancellationToken cancellationToken)
    {
        var components = await db.Componentes
            .Where(x => x.ProjetoId == projetoId)
            .Select(x => new { x.Id, x.ParentComponentId })
            .ToListAsync(cancellationToken);

        var ids = new List<Guid> { rootId };
        var queue = new Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            foreach (var child in components.Where(x => x.ParentComponentId == currentId))
            {
                ids.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }

        return ids;
    }
}
