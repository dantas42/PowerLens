import type { LinkTarget } from "./types";

function escapeHtml(value: string) {
  return value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

function escapeRegExp(value: string) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

export function renderMarkdownWithLinks(markdown: string, targets: LinkTarget[]) {
  const escaped = escapeHtml(markdown);
  const linked = targets
    .sort((a, b) => Math.max(b.nome.length, b.nomeLogico.length) - Math.max(a.nome.length, a.nomeLogico.length))
    .reduce((content, target) => {
      const tokens = [target.nome, target.nomeLogico].filter(Boolean);
      return tokens.reduce((inner, token) => {
        const expression = new RegExp(`(?<![\\w])${escapeRegExp(token)}(?![\\w])`, "gi");
        return inner.replace(expression, `<a href="/componentes/${target.id}">$&</a>`);
      }, content);
    }, escaped);

  return linked
    .replace(/^# (.*)$/gm, "<h1>$1</h1>")
    .replace(/^## (.*)$/gm, "<h2>$1</h2>")
    .replace(/\n{2,}/g, "</p><p>")
    .replace(/\n/g, "<br />")
    .replace(/^/, "<p>")
    .replace(/$/, "</p>");
}
