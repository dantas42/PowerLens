"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, Building2, FolderKanban, Plus, Trash2 } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";
import { PowerLensApi } from "@/lib/api";

export default function HomePage() {
  const queryClient = useQueryClient();
  const [empresaNome, setEmpresaNome] = useState("");
  const [selectedEmpresaId, setSelectedEmpresaId] = useState("");
  const [projetoNome, setProjetoNome] = useState("");
  const [dataverseUrl, setDataverseUrl] = useState("");

  useEffect(() => {
    if (!window.localStorage.getItem("powerlens.token")) {
      window.location.href = "/login";
    }
  }, []);

  const empresas = useQuery({ queryKey: ["empresas"], queryFn: PowerLensApi.empresas });
  const projetos = useQuery({
    queryKey: ["projetos", selectedEmpresaId],
    queryFn: () => PowerLensApi.projetos(selectedEmpresaId),
    enabled: Boolean(selectedEmpresaId)
  });

  const createEmpresa = useMutation({
    mutationFn: PowerLensApi.createEmpresa,
    onSuccess: (empresa) => {
      setEmpresaNome("");
      setSelectedEmpresaId(empresa.id);
      queryClient.invalidateQueries({ queryKey: ["empresas"] });
    }
  });

  const deleteEmpresa = useMutation({
    mutationFn: PowerLensApi.deleteEmpresa,
    onSuccess: (_, empresaId) => {
      if (selectedEmpresaId === empresaId) {
        setSelectedEmpresaId("");
      }
      queryClient.invalidateQueries({ queryKey: ["empresas"] });
    }
  });

  const createProjeto = useMutation({
    mutationFn: () => PowerLensApi.createProjeto(selectedEmpresaId, projetoNome, dataverseUrl),
    onSuccess: () => {
      setProjetoNome("");
      setDataverseUrl("");
      queryClient.invalidateQueries({ queryKey: ["projetos", selectedEmpresaId] });
    }
  });

  const deleteProjeto = useMutation({
    mutationFn: PowerLensApi.deleteProjeto,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projetos", selectedEmpresaId] });
    }
  });

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-6xl flex-col gap-6 px-6 py-8">
      <header className="flex items-center justify-between border-b border-line pb-5">
        <div>
          <h1 className="text-2xl font-bold text-ink">PowerLens</h1>
          <p className="mt-1 text-sm text-ink/70">Entenda sua solucao Power Platform em minutos, nao em horas.</p>
        </div>
        <button
          className="rounded-md border border-line px-3 py-2 text-sm"
          onClick={() => {
            window.localStorage.removeItem("powerlens.token");
            window.location.href = "/login";
          }}
        >
          Sair
        </button>
      </header>

      <section className="grid gap-5 md:grid-cols-[360px_1fr]">
        <aside className="rounded-md border border-line bg-white p-4">
          <div className="mb-4 flex items-center gap-2">
            <Building2 size={18} />
            <h2 className="font-semibold">Empresas</h2>
          </div>
          <form
            className="mb-4 flex gap-2"
            onSubmit={(event) => {
              event.preventDefault();
              createEmpresa.mutate(empresaNome);
            }}
          >
            <input className="min-w-0 flex-1 rounded-md border border-line px-3 py-2 text-sm" placeholder="Cliente A" value={empresaNome} onChange={(event) => setEmpresaNome(event.target.value)} />
            <button className="rounded-md bg-moss px-3 py-2 text-white" aria-label="Criar empresa">
              <Plus size={18} />
            </button>
          </form>
          <div className="space-y-2">
            {empresas.data?.map((empresa) => (
              <div
                key={empresa.id}
                className={`flex w-full items-center justify-between rounded-md border px-3 py-2 text-left text-sm ${selectedEmpresaId === empresa.id ? "border-moss bg-mint" : "border-line"}`}
              >
                <button className="flex min-w-0 flex-1 items-center justify-between text-left" onClick={() => setSelectedEmpresaId(empresa.id)}>
                  <span className="truncate">{empresa.nome}</span>
                  <ArrowRight size={16} />
                </button>
                <button
                  className="ml-2 rounded-md p-1 text-red-700 hover:bg-red-50"
                  aria-label={`Excluir ${empresa.nome}`}
                  onClick={() => {
                    if (window.confirm(`Excluir a organizacao "${empresa.nome}" e todos os seus projetos?`)) {
                      deleteEmpresa.mutate(empresa.id);
                    }
                  }}
                >
                  <Trash2 size={16} />
                </button>
              </div>
            ))}
          </div>
        </aside>

        <section className="rounded-md border border-line bg-white p-4">
          <div className="mb-4 flex items-center gap-2">
            <FolderKanban size={18} />
            <h2 className="font-semibold">Projetos</h2>
          </div>
          {selectedEmpresaId ? (
            <>
              <form
                className="mb-5 grid gap-2 md:grid-cols-[1fr_1fr_auto]"
                onSubmit={(event) => {
                  event.preventDefault();
                  createProjeto.mutate();
                }}
              >
                <input className="rounded-md border border-line px-3 py-2 text-sm" placeholder="CRM Producao" value={projetoNome} onChange={(event) => setProjetoNome(event.target.value)} />
                <input className="rounded-md border border-line px-3 py-2 text-sm" placeholder="https://org.crm.dynamics.com" value={dataverseUrl} onChange={(event) => setDataverseUrl(event.target.value)} />
                <button className="rounded-md bg-moss px-4 py-2 text-sm font-medium text-white">Criar</button>
              </form>
              <div className="grid gap-3 md:grid-cols-2">
                {projetos.data?.map((projeto) => (
                  <div key={projeto.id} className="rounded-md border border-line p-4 transition hover:border-moss">
                    <div className="flex items-start justify-between gap-3">
                      <Link href={`/projetos/${projeto.id}`} className="min-w-0 flex-1">
                        <div className="truncate font-semibold">{projeto.nome}</div>
                        <div className="mt-2 truncate text-sm text-ink/65">{projeto.dataverseUrl || "Dataverse nao configurado"}</div>
                      </Link>
                      <button
                        className="rounded-md p-1 text-red-700 hover:bg-red-50"
                        aria-label={`Excluir ${projeto.nome}`}
                        onClick={() => {
                          if (window.confirm(`Excluir o projeto "${projeto.nome}" e todos os seus componentes?`)) {
                            deleteProjeto.mutate(projeto.id);
                          }
                        }}
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <p className="text-sm text-ink/65">Selecione ou crie uma empresa para gerenciar projetos.</p>
          )}
        </section>
      </section>
    </main>
  );
}
