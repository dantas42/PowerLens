using PowerLens.Api.Domain;

namespace PowerLens.Api.Services;

public sealed class DocumentationTemplateService
{
    public string CreateDefault(Componente component)
    {
        return component.Tipo switch
        {
            ComponentType.Entidade => $"""
                # {component.Nome}

                ## Objetivo
                (Nao documentada)

                ## Como funciona
                (Nao documentada)

                ## Regras de negocio
                (Nao documentada)

                ## Observacoes
                (Nao documentada)

                ## Problemas conhecidos
                (Nao documentada)
                """,
            ComponentType.Campo => $"""
                # {component.Nome}

                ## Objetivo
                (Nao documentado)

                ## Observacoes
                (Nao documentado)
                """,
            _ => $"""
                # {component.Nome}

                ## Descricao
                (Nao documentado)
                """
        };
    }
}
