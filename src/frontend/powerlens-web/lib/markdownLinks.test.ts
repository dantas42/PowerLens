import { describe, expect, it } from "vitest";
import { renderMarkdownWithLinks } from "./markdownLinks";

describe("renderMarkdownWithLinks", () => {
  it("links known entity display and logical names", () => {
    const html = renderMarkdownWithLinks("UC_Fatura consulta uc_cliente.", [
      { id: "invoice-id", nome: "UC_Fatura", nomeLogico: "uc_fatura", tipo: "Entidade" },
      { id: "customer-id", nome: "UC_Cliente", nomeLogico: "uc_cliente", tipo: "Entidade" }
    ]);

    expect(html).toContain('<a href="/componentes/invoice-id">UC_Fatura</a>');
    expect(html).toContain('<a href="/componentes/customer-id">uc_cliente</a>');
  });

  it("does not link partial words", () => {
    const html = renderMarkdownWithLinks("Contador nao deve criar link.", [
      { id: "account-id", nome: "Conta", nomeLogico: "account", tipo: "Entidade" }
    ]);

    expect(html).not.toContain("/componentes/account-id");
  });
});
