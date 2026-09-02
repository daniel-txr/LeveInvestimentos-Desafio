# Arquitetura e decisões técnicas

Este documento registra as principais decisões tomadas na criação deste projeto — a primeira
entrega do contrato — que servirão de referência para os próximos projetos da equipe.

## Camadas

```
┌──────────────────────────────────┐
│     LeveInvestimentos.Web        │  Controllers, Views (Razor), ViewModels, wwwroot
├──────────────────────────────────┤
│ LeveInvestimentos.Infrastructure │  EF Core, Repositories, SMTP, Hash, Seed
├──────────────────────────────────┤
│  LeveInvestimentos.Application   │  DTOs, Services (regras de negócio), Interfaces
├──────────────────────────────────┤
│     LeveInvestimentos.Domain     │  Entidades, Enums, Interfaces de repository
└──────────────────────────────────┘
```

A regra de dependência é sempre "de fora para dentro": `Web` depende de `Application` e
`Infrastructure`; `Infrastructure` depende de `Application`; `Application` depende de `Domain`;
`Domain` não depende de nada. Isso é reforçado pelas referências de projeto (`.csproj`) e pela
ausência de pacotes de EF Core/ASP.NET Core no `Domain`/`Application`.

## Padrões utilizados

- **Repository Pattern** (`IRepositorioBase<T>` + repositórios especializados): isola o EF Core
  do restante da aplicação e evita duplicação de código CRUD básico.

- **Service Layer** (`UsuarioService`, `TarefaService`): concentra as regras de negócio, mantendo
  os Controllers finos (responsabilidade única: traduzir HTTP ↔ chamada de serviço).

- **DTO / ViewModel**: a camada `Application` só conhece DTOs (sem `IFormFile`, sem nada de HTTP);
  a camada `Web` tem seus próprios ViewModels quando precisa de algo específico de UI (ex.:
  `UsuarioCadastroViewModel`, que carrega o `IFormFile` da foto). Isso evita "vazar" detalhes de
  framework web para dentro das regras de negócio.

- **Dependency Injection** nativa do ASP.NET Core para toda a cadeia
  Controller → Service → Repository → DbContext.

- **Fluent API** (`IEntityTypeConfiguration<T>`) em vez de Data Annotations no `DbContext`, para
  manter as entidades de `Domain` limpas de detalhes de mapeamento relacional.

## Autenticação e autorização

- **Cookie Authentication** do próprio ASP.NET Core (sem depender de identidade externa), com
  claims customizadas (`usuario_id`, `perfil`, `nome_completo`).

- **Hash de senha com PBKDF2** nativo do .NET — evita dependência de pacotes de terceiros só para hashing, 
  e evita o antipadrão de armazenar senha em texto puro ou com algoritmos fracos (MD5/SHA1 simples).

- **Autorização por política** (`SomenteGestor`), verificando a claim de perfil — usada nos
  endpoints de cadastro de usuário e cadastro de tarefa.

- Cada regra de autorização de **domínio** (ex.: "só posso atribuir tarefa ao meu próprio
  subordinado", "só o responsável pode concluir a tarefa") é validada dentro do `TarefaService`,
  não apenas na camada HTTP — isso a protege mesmo que a aplicação ganhe uma API no futuro.

## Tratamento de erros

- Regras de negócio violadas lançam `DominioException`, capturada nos Controllers e convertida em
  mensagem amigável via `ModelState`, sem vazar stack trace ao usuário.

- Falha no envio de e-mail **não interrompe** a operação principal (cadastro de tarefa, conclusão
  de tarefa): a notificação é uma funcionalidade desejável, não crítica, então seu envio é isolado
  em um bloco `try/catch` dedicado no `TarefaService`.

- Validações de entrada usam Data Annotations tanto no client-side (jQuery Validation Unobtrusive) 
  quanto no server-side (`ModelState.IsValid`).

## Banco de dados

- **Code First** com Entity Framework Core — o modelo em C# é a fonte da verdade; o schema SQL é
  gerado a partir dele (migrations).

- Índice único em `Usuarios.Email` (reforça em nível de banco a regra já validada em código).

- Índices em `Tarefas.UsuarioResponsavelId` e `Tarefas.UsuarioGestorId`, pois são exatamente os
  filtros usados nas duas listagens principais ("minhas tarefas" e "tarefas que criei").

- `DeleteBehavior.Restrict` nos relacionamentos de usuário/tarefa, para não permitir exclusões em
  cascata acidentais de um histórico de tarefas.

## Front-end

- **Razor** para renderização de HTML no servidor, conforme definido nos requisitos técnicos.

- **UIkit 3** via CDN como padrão visual (grid, cards, formulários, alerts, badges), sem a
  necessidade de um pipeline de build de front-end (webpack/vite) — mantém o projeto simples de
  rodar em qualquer máquina.

- **jQuery + jQuery Validation Unobtrusive** para validação client-side integrada nativamente com
  as Data Annotations do ASP.NET Core (`asp-validation-for`), evitando duplicar regras de
  validação em dois lugares.

## Por que não usar o ASP.NET Core Identity completo?

O requisito é enxuto (login por e-mail/senha, cadastro com campos específicos, perfil
gestor/subordinado). Usar o Identity completo traria tabelas, telas e conceitos (roles, claims
providers, confirmação de e-mail, tokens de reset de senha) que não fazem parte do escopo desta
entrega e aumentariam a complexidade sem benefício imediato. Optou-se por uma implementação
enxuta, porém com boas práticas de segurança (hash forte, cookie HttpOnly, HTTPS), que pode
evoluir para o Identity completo em uma entrega futura, caso o contrato exija, sem quebrar 
a arquitetura em camadas já estabelecida.
