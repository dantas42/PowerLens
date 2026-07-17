using PowerLens.Api.Domain;
using PowerLens.Api.Services;

namespace PowerLens.Api.Tests;

public sealed class DocumentationTemplateServiceTests
{
    [Fact]
    public void CreateDefault_ForEntity_IncludesPowerLensSections()
    {
        var service = new DocumentationTemplateService();
        var component = new Componente
        {
            Nome = "UC_Fatura",
            NomeLogico = "uc_fatura",
            Tipo = ComponentType.Entidade
        };

        var markdown = service.CreateDefault(component);

        Assert.Contains("# UC_Fatura", markdown);
        Assert.Contains("## Objetivo", markdown);
        Assert.Contains("## Regras de negocio", markdown);
        Assert.Contains("(Nao documentada)", markdown);
    }
}
