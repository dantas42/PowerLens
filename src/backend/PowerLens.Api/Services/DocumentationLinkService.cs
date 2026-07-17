using System.Text.RegularExpressions;
using PowerLens.Api.Domain;

namespace PowerLens.Api.Services;

public sealed class DocumentationLinkService
{
    public IReadOnlyList<LinkCandidate> FindTargets(string markdown, IReadOnlyCollection<Componente> components)
    {
        var matches = new List<LinkCandidate>();

        foreach (var component in components.OrderByDescending(x => Math.Max(x.Nome.Length, x.NomeLogico.Length)))
        {
            if (ContainsToken(markdown, component.Nome) || ContainsToken(markdown, component.NomeLogico))
            {
                matches.Add(new LinkCandidate(component.Id, component.Nome, component.NomeLogico, component.Tipo));
            }
        }

        return matches;
    }

    private static bool ContainsToken(string markdown, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return Regex.IsMatch(markdown, $@"(?<![\w]){Regex.Escape(token)}(?![\w])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

public sealed record LinkCandidate(Guid Id, string Nome, string NomeLogico, ComponentType Tipo);
