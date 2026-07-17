import type { AuthResponse, Component, ComponentDetails, Documentation, Empresa, LinkTarget, Projeto } from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5216";

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = typeof window !== "undefined" ? window.localStorage.getItem("powerlens.token") : null;
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers
    }
  });

  if (!response.ok) {
    throw new ApiError(response.status, await response.text());
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export const PowerLensApi = {
  login: (email: string, password: string) => api<AuthResponse>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
  register: (nome: string, email: string, password: string) => api<AuthResponse>("/api/auth/register", { method: "POST", body: JSON.stringify({ nome, email, password }) }),
  me: () => api<AuthResponse["user"]>("/api/auth/me"),
  empresas: () => api<Empresa[]>("/api/empresas"),
  createEmpresa: (nome: string) => api<Empresa>("/api/empresas", { method: "POST", body: JSON.stringify({ nome }) }),
  deleteEmpresa: (empresaId: string) => api<void>(`/api/empresas/${empresaId}`, { method: "DELETE" }),
  projetos: (empresaId: string) => api<Projeto[]>(`/api/empresas/${empresaId}/projetos`),
  createProjeto: (empresaId: string, nome: string, dataverseUrl?: string) =>
    api<Projeto>(`/api/empresas/${empresaId}/projetos`, { method: "POST", body: JSON.stringify({ nome, dataverseUrl }) }),
  deleteProjeto: (projetoId: string) => api<void>(`/api/projetos/${projetoId}`, { method: "DELETE" }),
  saveDataverseConnection: (projetoId: string, payload: { dataverseUrl: string; tenantId: string; clientId: string; clientSecret: string }) =>
    api(`/api/projetos/${projetoId}/dataverse-connection`, { method: "PUT", body: JSON.stringify(payload) }),
  syncDataverse: (projetoId: string) => api<{ entities: number; fields: number; choices: number; relationships: number }>(`/api/projetos/${projetoId}/sync/dataverse`, { method: "POST" }),
  componentes: (projetoId: string, search = "", tipo = "Entidade") =>
    api<Component[]>(`/api/projetos/${projetoId}/componentes?tipo=${tipo}&search=${encodeURIComponent(search)}`),
  undocumented: (projetoId: string) => api<Component[]>(`/api/projetos/${projetoId}/itens-sem-documentacao`),
  linkTargets: (projetoId: string) => api<LinkTarget[]>(`/api/projetos/${projetoId}/link-targets`),
  component: (componentId: string) => api<ComponentDetails>(`/api/componentes/${componentId}`),
  deleteComponent: (componentId: string) => api<void>(`/api/componentes/${componentId}`, { method: "DELETE" }),
  documentation: (componentId: string) => api<Documentation>(`/api/componentes/${componentId}/documentacao`),
  updateDocumentation: (componentId: string, conteudoMarkdown: string) =>
    api<Documentation>(`/api/componentes/${componentId}/documentacao`, { method: "PUT", body: JSON.stringify({ conteudoMarkdown }) })
};
