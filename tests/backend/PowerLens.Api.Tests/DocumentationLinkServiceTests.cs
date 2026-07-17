using PowerLens.Api.Domain;
using PowerLens.Api.Services;

namespace PowerLens.Api.Tests;

public sealed class DocumentationLinkServiceTests
{
    [Fact]
    public void FindTargets_MatchesDisplayAndLogicalNames()
    {
        var service = new DocumentationLinkService();
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var components = new[]
        {
            new Componente { Id = invoiceId, Nome = "UC_Fatura", NomeLogico = "uc_fatura", Tipo = ComponentType.Entidade },
            new Componente { Id = customerId, Nome = "UC_Cliente", NomeLogico = "uc_cliente", Tipo = ComponentType.Entidade }
        };

        var targets = service.FindTargets("UC_Fatura relaciona-se com uc_cliente.", components);

        Assert.Collection(
            targets.OrderBy(x => x.Nome),
            target => Assert.Equal(customerId, target.Id),
            target => Assert.Equal(invoiceId, target.Id));
    }

    [Fact]
    public void FindTargets_DoesNotMatchPartialWords()
    {
        var service = new DocumentationLinkService();
        var components = new[]
        {
            new Componente { Id = Guid.NewGuid(), Nome = "Conta", NomeLogico = "account", Tipo = ComponentType.Entidade }
        };

        var targets = service.FindTargets("A palavra Contador nao deve criar link.", components);

        Assert.Empty(targets);
    }
}
