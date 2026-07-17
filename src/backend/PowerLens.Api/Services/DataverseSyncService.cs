using Microsoft.EntityFrameworkCore;
using PowerLens.Api.Data;
using PowerLens.Api.Domain;

namespace PowerLens.Api.Services;

public sealed class DataverseSyncService(PowerLensDbContext db, DocumentationTemplateService templates)
{
    public async Task<SyncResult> SyncAsync(Guid projetoId, CancellationToken cancellationToken)
    {
        var projetoExists = await db.Projetos.AnyAsync(x => x.Id == projetoId, cancellationToken);
        if (!projetoExists)
        {
            throw new InvalidOperationException("Projeto nao encontrado.");
        }

        var invoice = await UpsertComponentAsync(projetoId, null, ComponentType.Entidade, "UC_Fatura", "uc_fatura", "Entidade de faturas do cliente.", cancellationToken);
        var customer = await UpsertComponentAsync(projetoId, null, ComponentType.Entidade, "UC_Cliente", "uc_cliente", "Entidade de clientes.", cancellationToken);
        var statusChoice = await UpsertComponentAsync(projetoId, invoice.Id, ComponentType.Choice, "Status da Fatura", "uc_fatura_statuscode", "Opcoes de status da fatura.", cancellationToken);

        var fields = new[]
        {
            await UpsertComponentAsync(projetoId, invoice.Id, ComponentType.Campo, "Numero da Fatura", "uc_numero", "Numero unico da fatura.", cancellationToken),
            await UpsertComponentAsync(projetoId, invoice.Id, ComponentType.Campo, "Cliente", "uc_clienteid", "Cliente relacionado a fatura.", cancellationToken),
            await UpsertComponentAsync(projetoId, invoice.Id, ComponentType.Campo, "Valor Total", "uc_valortotal", "Valor total da fatura.", cancellationToken),
            await UpsertComponentAsync(projetoId, customer.Id, ComponentType.Campo, "Nome", "uc_nome", "Nome do cliente.", cancellationToken),
            statusChoice
        };

        await UpsertRelationshipAsync(invoice.Id, customer.Id, RelationshipType.RelacionaCom, cancellationToken);
        foreach (var field in fields.Where(x => x.Tipo == ComponentType.Campo))
        {
            await UpsertRelationshipAsync(field.ParentComponentId!.Value, field.Id, RelationshipType.PossuiCampo, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SyncResult(2, fields.Count(x => x.Tipo == ComponentType.Campo), 1, 1 + fields.Count(x => x.Tipo == ComponentType.Campo));
    }

    private async Task<Componente> UpsertComponentAsync(
        Guid projetoId,
        Guid? parentComponentId,
        ComponentType type,
        string name,
        string logicalName,
        string? description,
        CancellationToken cancellationToken)
    {
        var component = await db.Componentes
            .Include(x => x.Documentacao)
            .FirstOrDefaultAsync(x => x.ProjetoId == projetoId && x.Tipo == type && x.NomeLogico == logicalName, cancellationToken);

        if (component is null)
        {
            component = new Componente
            {
                Id = Guid.NewGuid(),
                ProjetoId = projetoId,
                ParentComponentId = parentComponentId,
                Tipo = type,
                Nome = name,
                NomeLogico = logicalName,
                Descricao = description,
                Documentacao = new Documentacao
                {
                    Id = Guid.NewGuid(),
                    ConteudoMarkdown = string.Empty,
                    IsGenerated = true
                }
            };
            component.Documentacao.ConteudoMarkdown = templates.CreateDefault(component);
            db.Componentes.Add(component);
        }
        else
        {
            component.ParentComponentId = parentComponentId;
            component.Nome = name;
            component.Descricao = description;
        }

        return component;
    }

    private async Task UpsertRelationshipAsync(Guid originId, Guid targetId, RelationshipType type, CancellationToken cancellationToken)
    {
        var exists = await db.Relacionamentos.AnyAsync(
            x => x.OrigemComponenteId == originId && x.DestinoComponenteId == targetId && x.TipoRelacionamento == type,
            cancellationToken);

        if (!exists)
        {
            db.Relacionamentos.Add(new Relacionamento
            {
                Id = Guid.NewGuid(),
                OrigemComponenteId = originId,
                DestinoComponenteId = targetId,
                TipoRelacionamento = type
            });
        }
    }
}

public sealed record SyncResult(int Entities, int Fields, int Choices, int Relationships);
