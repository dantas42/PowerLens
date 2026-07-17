"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DatabaseZap, FileWarning, RefreshCw, Search, Trash2 } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { PowerLensApi } from "@/lib/api";

export default function ProjectPage() {
  const { projetoId } = useParams<{ projetoId: string }>();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [connection, setConnection] = useState({ dataverseUrl: "", tenantId: "", clientId: "", clientSecret: "" });

  const components = useQuery({
    queryKey: ["componentes", projetoId, search],
    queryFn: () => PowerLensApi.componentes(projetoId, search)
  });

  const undocumented = useQuery({
    queryKey: ["undocumented", projetoId],
    queryFn: () => PowerLensApi.undocumented(projetoId)
  });

  const saveConnection = useMutation({
    mutationFn: () => PowerLensApi.saveDataverseConnection(projetoId, connection)
  });

  const sync = useMutation({
    mutationFn: () => PowerLensApi.syncDataverse(projetoId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["componentes", projetoId] });
      queryClient.invalidateQueries({ queryKey: ["undocumented", projetoId] });
    }
  });

  const deleteComponent = useMutation({
    mutationFn: PowerLensApi.deleteComponent,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["componentes", projetoId] });
      queryClient.invalidateQueries({ queryKey: ["undocumented", projetoId] });
    }
  });

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-7xl flex-col gap-5 px-6 py-8">
      <header className="flex flex-wrap items-center justify-between gap-3 border-b border-line pb-5">
        <div>
          <Link href="/" className="text-sm text-moss">
            Empresas e projetos
          </Link>
          <h1 className="mt-1 text-2xl font-bold">Dashboard do projeto</h1>
        </div>
        <button className="inline-flex items-center gap-2 rounded-md bg-moss px-4 py-2 text-sm font-medium text-white" onClick={() => sync.mutate()} disabled={sync.isPending}>
          <RefreshCw size={16} />
          {sync.isPending ? "Sincronizando..." : "Sincronizar Dataverse"}
        </button>
      </header>

      <section className="grid gap-5 lg:grid-cols-[380px_1fr]">
        <aside className="space-y-5">
          <section className="rounded-md border border-line bg-white p-4">
            <div className="mb-4 flex items-center gap-2">
              <DatabaseZap size={18} />
              <h2 className="font-semibold">Conexao Dataverse</h2>
            </div>
            <form
              className="space-y-3"
              onSubmit={(event) => {
                event.preventDefault();
                saveConnection.mutate();
              }}
            >
              <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="URL Dataverse" value={connection.dataverseUrl} onChange={(event) => setConnection((current) => ({ ...current, dataverseUrl: event.target.value }))} />
              <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Tenant ID" value={connection.tenantId} onChange={(event) => setConnection((current) => ({ ...current, tenantId: event.target.value }))} />
              <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Client ID" value={connection.clientId} onChange={(event) => setConnection((current) => ({ ...current, clientId: event.target.value }))} />
              <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Client Secret" type="password" value={connection.clientSecret} onChange={(event) => setConnection((current) => ({ ...current, clientSecret: event.target.value }))} />
              <button className="w-full rounded-md border border-moss px-4 py-2 text-sm font-medium text-moss">Salvar conexao</button>
            </form>
          </section>

          <section className="rounded-md border border-line bg-white p-4">
            <div className="mb-3 flex items-center gap-2">
              <FileWarning size={18} />
              <h2 className="font-semibold">Itens sem documentacao</h2>
            </div>
            <div className="text-3xl font-bold">{undocumented.data?.length ?? 0}</div>
            <div className="mt-3 space-y-2">
              {undocumented.data?.slice(0, 6).map((item) => (
                <Link key={item.id} href={`/componentes/${item.id}`} className="block rounded-md border border-line px-3 py-2 text-sm hover:border-moss">
                  {item.nome}
                </Link>
              ))}
            </div>
          </section>
        </aside>

        <section className="rounded-md border border-line bg-white p-4">
          <div className="mb-4 flex items-center gap-2">
            <Search size={18} />
            <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Buscar entidade por nome ou nome logico" value={search} onChange={(event) => setSearch(event.target.value)} />
          </div>
          <div className="overflow-hidden rounded-md border border-line">
            <table className="w-full border-collapse text-left text-sm">
              <thead className="bg-paper text-ink/70">
                <tr>
                  <th className="px-3 py-2 font-medium">Entidade</th>
                  <th className="px-3 py-2 font-medium">Nome logico</th>
                  <th className="px-3 py-2 font-medium">Documentacao</th>
                </tr>
              </thead>
              <tbody>
                {components.data?.map((component) => (
                  <tr key={component.id} className="border-t border-line">
                    <td className="px-3 py-2">
                      <Link href={`/componentes/${component.id}`} className="font-medium text-moss">
                        {component.nome}
                      </Link>
                    </td>
                    <td className="px-3 py-2 text-ink/70">{component.nomeLogico}</td>
                    <td className="px-3 py-2">
                      <div className="flex items-center justify-between gap-3">
                        <span>{component.isDocumentationGenerated ? "Pendente" : "Editada"}</span>
                        <button
                          className="rounded-md p-1 text-red-700 hover:bg-red-50"
                          aria-label={`Excluir ${component.nome}`}
                          onClick={() => {
                            if (window.confirm(`Excluir a tabela "${component.nome}" e seus campos?`)) {
                              deleteComponent.mutate(component.id);
                            }
                          }}
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </section>
    </main>
  );
}
