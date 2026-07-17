export type ComponentType = "Entidade" | "Campo" | "Choice" | "Plugin" | "Flow" | "JavaScript" | "PCF" | "Workflow";

export type AuthResponse = {
  token: string;
  user: User;
};

export type User = {
  id: string;
  nome: string;
  email: string;
};

export type Empresa = {
  id: string;
  nome: string;
};

export type Projeto = {
  id: string;
  empresaId: string;
  nome: string;
  dataverseUrl?: string | null;
  hasDataverseConnection: boolean;
};

export type Component = {
  id: string;
  projetoId: string;
  parentComponentId?: string | null;
  tipo: ComponentType;
  nome: string;
  nomeLogico: string;
  descricao?: string | null;
  isDocumentationGenerated: boolean;
};

export type Relationship = {
  id: string;
  origemComponenteId: string;
  destinoComponenteId: string;
  tipoRelacionamento: string;
  destinoNome: string;
  destinoNomeLogico: string;
};

export type ComponentDetails = {
  component: Component;
  fields: Component[];
  relationships: Relationship[];
  history: { id: string; commit?: string | null; data: string; descricao: string }[];
};

export type Documentation = {
  id: string;
  componenteId: string;
  conteudoMarkdown: string;
  isGenerated: boolean;
  updatedAt: string;
};

export type LinkTarget = {
  id: string;
  nome: string;
  nomeLogico: string;
  tipo: ComponentType;
};
