"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Braces, Clock3, FileText, GitBranch, PlugZap, Save, Trash2, Workflow } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";
import { MarkdownEditor } from "@/components/MarkdownEditor";
import { PowerLensApi } from "@/lib/api";
import { renderMarkdownWithLinks } from "@/lib/markdownLinks";

type Tab = "docs" | "fields" | "relationships" | "history" | "plugins" | "flows";

export default function ComponentPage() {
  const { componentId } = useParams<{ componentId: string }>();
  const queryClient = useQueryClient();
  const [tab, setTab] = useState<Tab>("docs");
  const details = useQuery({ queryKey: ["component", componentId], queryFn: () => PowerLensApi.component(componentId) });
  const documentation = useQuery({ queryKey: ["documentation", componentId], queryFn: () => PowerLensApi.documentation(componentId) });
  const linkTargets = useQuery({
    queryKey: ["link-targets", details.data?.component.projetoId],
    queryFn: () => PowerLensApi.linkTargets(details.data!.component.projetoId),
    enabled: Boolean(details.data?.component.projetoId)
  });
  const [draft, setDraft] = useState<string | null>(null);
  const markdown = draft ?? documentation.data?.conteudoMarkdown ?? "";

  const renderedMarkdown = useMemo(() => renderMarkdownWithLinks(markdown, linkTargets.data ?? []), [markdown, linkTargets.data]);

  const updateDocumentation = useMutation({
    mutationFn: () => PowerLensApi.updateDocumentation(componentId, markdown),
    onSuccess: () => {
      setDraft(null);
      queryClient.invalidateQueries({ queryKey: ["documentation", componentId] });
      queryClient.invalidateQueries({ queryKey: ["component", componentId] });
    }
  });

  const deleteComponent = useMutation({
    mutationFn: () => PowerLensApi.deleteComponent(componentId),
    onSuccess: () => {
      if (component?.projetoId) {
        window.location.href = `/projetos/${component.projetoId}`;
      }
    }
  });

  const component = details.data?.component;

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-7xl flex-col gap-5 px-6 py-8">
      <header className="flex flex-wrap items-center justify-between gap-3 border-b border-line pb-5">
        <div>
          {component ? (
            <Link href={`/projetos/${component.projetoId}`} className="text-sm text-moss">
              Voltar ao projeto
            </Link>
          ) : null}
          <h1 className="mt-1 text-2xl font-bold">{component?.nome ?? "Componente"}</h1>
          <p className="text-sm text-ink/65">{component?.nomeLogico}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            className="inline-flex items-center gap-2 rounded-md border border-red-200 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50"
            onClick={() => {
              if (window.confirm(`Excluir a tabela "${component?.nome ?? "selecionada"}" e seus campos?`)) {
                deleteComponent.mutate();
              }
            }}
            disabled={deleteComponent.isPending}
          >
            <Trash2 size={16} />
            Excluir
          </button>
          <button className="inline-flex items-center gap-2 rounded-md bg-moss px-4 py-2 text-sm font-medium text-white" onClick={() => updateDocumentation.mutate()} disabled={updateDocumentation.isPending}>
            <Save size={16} />
            Salvar documentacao
          </button>
        </div>
      </header>

      <nav className="flex flex-wrap gap-2">
        <TabButton active={tab === "docs"} onClick={() => setTab("docs")} icon={<FileText size={16} />} label="Documentacao" />
        <TabButton active={tab === "fields"} onClick={() => setTab("fields")} icon={<Braces size={16} />} label="Campos" />
        <TabButton active={tab === "relationships"} onClick={() => setTab("relationships")} icon={<GitBranch size={16} />} label="Relacionamentos" />
        <TabButton active={tab === "history"} onClick={() => setTab("history")} icon={<Clock3 size={16} />} label="Historico" />
        <TabButton active={tab === "plugins"} onClick={() => setTab("plugins")} icon={<PlugZap size={16} />} label="Plugins" />
        <TabButton active={tab === "flows"} onClick={() => setTab("flows")} icon={<Workflow size={16} />} label="Flows" />
      </nav>

      {tab === "docs" ? (
        <section className="grid gap-5 lg:grid-cols-2">
          <div>
            <h2 className="mb-2 font-semibold">Editor</h2>
            <MarkdownEditor value={markdown} onChange={setDraft} />
          </div>
          <div>
            <h2 className="mb-2 font-semibold">Preview com links</h2>
            <article className="prose-preview min-h-[420px] rounded-md border border-line bg-white p-4 text-sm leading-6" dangerouslySetInnerHTML={{ __html: renderedMarkdown }} />
          </div>
        </section>
      ) : null}

      {tab === "fields" ? (
        <DataList title="Campos" rows={details.data?.fields.map((field) => [field.nome, field.nomeLogico, field.descricao ?? ""]) ?? []} />
      ) : null}

      {tab === "relationships" ? (
        <DataList title="Relacionamentos" rows={details.data?.relationships.map((relationship) => [relationship.tipoRelacionamento, relationship.destinoNome, relationship.destinoNomeLogico]) ?? []} />
      ) : null}

      {tab === "history" ? <Placeholder title="Historico" text="Integracao Git e auditoria entrarao apos o MVP de sincronizacao." /> : null}
      {tab === "plugins" ? <Placeholder title="Plugins" text="Plugins serao relacionados a entidade na proxima evolucao." /> : null}
      {tab === "flows" ? <Placeholder title="Flows" text="Power Automate sera integrado depois da base Dataverse." /> : null}
    </main>
  );
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button className={`inline-flex items-center gap-2 rounded-md border px-3 py-2 text-sm ${active ? "border-moss bg-mint text-moss" : "border-line bg-white"}`} onClick={onClick}>
      {icon}
      {label}
    </button>
  );
}

function DataList({ title, rows }: { title: string; rows: string[][] }) {
  return (
    <section className="rounded-md border border-line bg-white p-4">
      <h2 className="mb-3 font-semibold">{title}</h2>
      <div className="divide-y divide-line rounded-md border border-line">
        {rows.map((row) => (
          <div key={row.join("|")} className="grid gap-2 px-3 py-2 text-sm md:grid-cols-3">
            {row.map((cell) => (
              <span key={cell}>{cell}</span>
            ))}
          </div>
        ))}
      </div>
    </section>
  );
}

function Placeholder({ title, text }: { title: string; text: string }) {
  return (
    <section className="rounded-md border border-line bg-white p-6">
      <h2 className="font-semibold">{title}</h2>
      <p className="mt-2 text-sm text-ink/65">{text}</p>
    </section>
  );
}
