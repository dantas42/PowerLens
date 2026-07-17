# PowerLens

Entenda sua solucao Power Platform em minutos, nao em horas.

PowerLens e um MVP full-stack para documentar solucoes Dataverse com a entidade como centro de navegacao.

## Stack

- Backend: ASP.NET Core 9, C#, ASP.NET Identity, JWT, EF Core
- Frontend: Next.js, TypeScript, Tailwind CSS, TanStack Query, MDX Editor
- Banco: PostgreSQL unico, separado por empresa e projeto

## Estrutura

- `src/backend/PowerLens.Api`: API, dominio, persistencia e sincronizacao
- `src/frontend/powerlens-web`: aplicacao web
- `tests/backend/PowerLens.Api.Tests`: testes do backend
- `docker-compose.yml`: PostgreSQL local

## Rodando localmente

1. Suba o banco:

```powershell
docker compose up -d postgres
```

2. Restaure e rode a API:

```powershell
dotnet restore PowerLens.slnx
dotnet run --project src/backend/PowerLens.Api/PowerLens.Api.csproj
```

3. Instale e rode o frontend:

```powershell
cd src/frontend/powerlens-web
npm.cmd install
npm.cmd run dev
```

O frontend ja vem configurado para chamar a API em `http://localhost:5216` por `src/frontend/powerlens-web/.env.local`.

4. Abra `http://localhost:3000`, crie uma conta, uma empresa e um projeto.

## MVP atual

- Login e cadastro com JWT
- Empresas e projetos
- Configuracao de conexao Dataverse via app registration
- Sincronizacao inicial mockada com entidades, campos, choice e relacionamento
- Documentacao Markdown por componente
- Links automaticos para entidades e componentes conhecidos
- Painel de itens sem documentacao
