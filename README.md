# Leve Investimentos — Sistema de Cadastro de Usuários e Agendamento de Tarefas

Sistema web desenvolvido em **ASP.NET Core MVC** com **Razor** e **UIkit**, com autenticação por
e-mail/senha, cadastro de usuários (gestores e subordinados) e agendamento de tarefas com
notificação por e-mail.

Este projeto é a primeira entrega de um novo contrato e define o **padrão arquitetural e de
código** a ser seguido pela equipe nos próximos projetos. Por isso, a estrutura prioriza
separação de responsabilidades, código coeso e extensível.

## Sumário

- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Arquitetura do projeto](#arquitetura-do-projeto)
- [Funcionalidades implementadas](#funcionalidades-implementadas)
- [Pré-requisitos](#pré-requisitos)
- [Passo a passo para executar o projeto](#passo-a-passo-para-executar-o-projeto)
- [Usuário padrão inicial](#usuário-padrão-inicial)
- [Configuração de e-mail](#configuração-de-e-mail)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Possíveis evoluções](#possíveis-evoluções)

## Tecnologias utilizadas

| Camada                | Tecnologia                                             |
|-----------------------|--------------------------------------------------------|
| Back-end              | C# / .NET 10 (LTS) / ASP.NET Core MVC                  |
| Renderização de UI    | Razor (Views)                                          |
| Estilização           | UIkit 3 (via CDN)                                      |
| Validação client-side | jQuery + jQuery Validation (Unobtrusive)               |
| Banco de dados        | SQL Server + Entity Framework Core 10 (Code First)     |
| Autenticação          | Cookie Authentication (ASP.NET Core) com hash PBKDF2   |
| Envio de e-mail       | SMTP nativo (`System.Net.Mail`)                        |

## Arquitetura do projeto

O projeto foi organizado em **4 camadas** (Clean Architecture / N-Layer), como base para os
próximos projetos da equipe:

```
LeveInvestimentos.Domain          -> Entidades, Enums e contratos de repositório (não depende de nada)
LeveInvestimentos.Application     -> Regras de negócio, DTOs, contratos de serviço (depende de Domain)
LeveInvestimentos.Infrastructure  -> EF Core, repositórios, e-mail, hashing (depende de Application)
LeveInvestimentos.Web             -> Controllers, Views (Razor), ViewModels (depende de Application/Infrastructure)
```

**Por que essa separação?**
- **Domain** não conhece banco de dados, HTTP ou qualquer framework — só regras e contratos puros.
- **Application** concentra as regras de negócio de forma testável, sem depender de EF Core ou ASP.NET Core.
- **Infrastructure** implementa os detalhes técnicos (EF Core, SMTP, hashing) atrás de interfaces
  definidas em Application — trocar SQL Server por outro banco, ou SMTP por outro provedor de
  e-mail, não deve exigir alterar regra de negócio nenhuma.
- **Web** é só a apresentação: Controllers finos, que apenas orquestram chamadas aos
  serviços de Application e traduzem o resultado em Views/ViewModels.

Mais detalhes de decisões de design em [`docs/ARQUITETURA.md`](docs/ARQUITETURA.md).

## Funcionalidades implementadas

- **Autenticação** por e-mail e senha (cookie), com senha protegida por hash PBKDF2 (nunca
  armazenada em texto puro).
- **Cadastro de usuários** (nome completo, data de nascimento, telefone fixo, telefone celular,
  e-mail, endereço e foto), restrito a usuários com perfil **Gestor**.
- **Identificação de perfil** (Gestor / Subordinado), com autorização por política (`[Authorize(Policy = "SomenteGestor")]`).
- **Agendamento de tarefas**: gestores cadastram tarefas (mensagem + data limite) para seus
  subordinados e acompanham o andamento.
- **Conclusão de tarefas** pelo próprio subordinado responsável.
- **Notificações por e-mail**:
  - o subordinado recebe um e-mail quando uma nova tarefa é atribuída a ele;
  - o gestor recebe um e-mail quando a tarefa é concluída.
  - Por padrão, o envio roda em **modo simulado** (grava no log), para não exigir SMTP configurado
    em ambiente de dev — veja [Configuração de e-mail](#configuração-de-e-mail).
- **Validações** client-side (jQuery Validation) e server-side (Data Annotations + regras de
  negócio na camada Application).
- **Usuário gestor padrão** criado automaticamente na primeira execução.

## Pré-requisitos

- [.NET SDK 10.0+ (LTS)](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local, Docker ou Azure SQL) — [SQL Server 2022 Developer Edition](https://www.microsoft.com/sql-server/sql-server-downloads) ou
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SenhaForte123!" \
    -p 1433:1433 --name sqlserver-leve -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- Git

## Passo a passo para executar o projeto

### 1. Clonar o repositório

```bash
git clone https://github.com/daniel-txr/LeveInvestimentos-Desafio.git
cd LeveInvestimentos-Desafio
```

### 2. Configurar a string de conexão

Edite `src/LeveInvestimentos.Web/appsettings.json`
e ajuste `ConnectionStrings:DefaultConnection` com os dados do seu SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=LeveInvestimentosDb;User Id=sa;Password=SenhaForte123!;TrustServerCertificate=True;"
}
```

Usando User Secrets (recomendado, evita gravar senha no repositório):

```bash
cd src/LeveInvestimentos.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=LeveInvestimentosDb;User Id=sa;Password=SenhaForte123!;TrustServerCertificate=True;"
```

### 3. Restaurar os pacotes

```bash
cd LeveInvestimentos-Desafio
dotnet restore
```

### 4. Criar o banco de dados

**Migrations do Entity Framework Core:**

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver a ferramenta instalada
cd src/LeveInvestimentos.Web

dotnet ef migrations add InitialCreate \
  --project ../LeveInvestimentos.Infrastructure \
  --startup-project .

dotnet ef database update \
  --project ../LeveInvestimentos.Infrastructure \
  --startup-project .
```

### 5. Executar a aplicação

```bash
cd src/LeveInvestimentos.Web
dotnet run
```

A aplicação sobe por padrão em `http://localhost:5140`. Acesse essa
URL no navegador — você será redirecionado para a tela de login.

### 6. Login inicial

Use as credenciais do usuário gestor padrão (veja a próxima seção), cadastre os primeiros
subordinados em **Usuários → Novo usuário** e, em seguida, atribua tarefas em **Tarefas → Nova
tarefa**.

## Usuário padrão inicial

Criado automaticamente na primeira execução da aplicação (idempotente — não duplica se já existir):

| Campo  | Valor                            |
|--------|----------------------------------|
| E-mail | `ti@leveinvestimentos.com.br`    |
| Senha  | `teste123`                       |
| Perfil | Gestor                           |

> Em um ambiente produtivo, recomenda-se forçar a troca dessa senha no primeiro acesso — ponto
> de evolução natural, listado em [Possíveis evoluções](#possíveis-evoluções).

## Logs

A aplicação usa **Serilog**, gravando em dois destinos simultaneamente:

- **Console**: útil durante o desenvolvimento (`dotnet run`).
- **Arquivo**: `src/LeveInvestimentos.Web/Logs/log-AAAAMMDD.txt`, com um arquivo novo por dia
  (rotação diária) e retenção dos últimos 14 dias. A pasta `Logs/` é criada automaticamente na primeira execução.

Isso inclui, por exemplo, falhas no envio de e-mail de notificação (nível `Warning`, com a
exceção original) e falhas inesperadas durante o startup da aplicação (nível `Fatal`).

Os níveis mínimos de log são configuráveis na seção `Serilog` do `appsettings.json` /
`appsettings.Development.json`, sem necessidade de alterar código.

## Configuração de e-mail

Por padrão, `EmailSettings:ModoSimulado` está `true` em `appsettings.json`: os e-mails não são
enviados de fato, apenas registrados no log da aplicação — assim é possível rodar localmente sem
precisar configurar um servidor SMTP.

Para ativar o envio real, configure um servidor SMTP válido (Gmail, Outlook, SendGrid SMTP, etc.)
e ajuste:

```json
"EmailSettings": {
  "ModoSimulado": false,
  "Host": "",
  "Porta": 587,
  "Usuario": "",
  "Senha": "",
  "RemetenteNome": "Leve Investimentos",
  "RemetenteEmail": "contato@leveinvestimentos.com.br",
  "UsarSsl": true
}
```

Por segurança, prefira configurar `EmailSettings:Senha` via User Secrets ou variável de ambiente
em vez de deixá-la em `appsettings.json`.

## Estrutura de pastas

```
LeveInvestimentos-Desafio/
├── LeveInvestimentos.sln
├── src/
│   ├── LeveInvestimentos.Domain/          # Entidades, Enums, Interfaces de repositório
│   ├── LeveInvestimentos.Application/     # DTOs, regras de negócio (Services), contratos
│   ├── LeveInvestimentos.Infrastructure/  # EF Core, Repositórios, E-mail, Hash, Seed
│   └── LeveInvestimentos.Web/             # Controllers, Views (Razor), ViewModels, wwwroot
└── docs/                                  # Documentação de arquitetura e decisões técnicas
```

## Possíveis evoluções

Fora do escopo desta primeira entrega, mas mapeadas para as próximas demandas do contrato (por ordem de prioridade/complexidade):

- Política de expiração/força de senha e forçar alteração de senha do usuário padrão.
- Máscara nos campos de telefone e validação da mesma.
- Filtros nas listagens de usuários e tarefas.
- Perfil master para gerenciamento geral.
- Job em background para marcar tarefas como atrasadas automaticamente.
- Testes automatizados para a camada `Application`, que já foi desenhada sem dependências
  de infraestrutura justamente para facilitar isso.
