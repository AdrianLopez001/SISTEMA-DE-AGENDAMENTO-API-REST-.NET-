# 📌 Status de Desenvolvimento: Sistema de Agendamento (API REST .NET)

> **Documento de Continuidade:** registra o que foi feito em cada fase e serve de guia de estudo do projeto.

## 🎯 OBJETIVO DE ESTUDO
Reforçar o desenvolvimento de APIs REST aprendendo uma linguagem nova: **C# com .NET**.

## 🧑‍💻 O QUE EU FIZ
- Defini a **arquitetura** (Clean Architecture em 4 camadas) e a **estrutura de pastas e arquivos**.
- Configurei a **solução, os projetos e as referências** entre as camadas.
- Fiz a **modelagem do domínio**: entidades, enums, exceções e interfaces de repositório.
- Defini as **regras de negócio e os requisitos**: conflito de horário, grade de disponibilidade e cancelamento com 2h de antecedência.
- Fiz o **planejamento em fases** e a documentação do projeto.

## 🤖 ONDE A IA AUXILIOU
- **Sintaxe da linguagem**, já que ainda estou aprendendo C#.
- **Implementação** de partes da infraestrutura (EF Core, Identity, JWT) e da camada de API.
- **Testes unitários** (xUnit + Moq).

## 📚 O QUE AINDA QUERO EVOLUIR
- Escrever sozinho as próximas funcionalidades e testes, com menos apoio da IA.
- Testes de integração, Docker e paginação (lista completa no README).

---

## 🟢 STATUS: TODAS AS FASES CONCLUÍDAS

`dotnet build`: **0 Aviso(s), 0 Erro(s)** · `dotnet test`: **20 aprovados, 0 falhas**

---

### ✅ Fase 1: Setup da Solução e Arquitetura
- Solução `MinhaAgendaApi.slnx` com os projetos `Domain`, `Application`, `Infrastructure`, `Presentation` e `UnitTests`.
- Referências: `Application ➔ Domain` · `Infrastructure ➔ Domain, Application` · `Presentation ➔ Application, Infrastructure` · `UnitTests ➔ Domain, Application`.

### ✅ Fase 2: Domínio (`src/Core/Domain`)
- **Enums**: `TipoPerfil`, `StatusAgendamento`.
- **Exceção**: `DomainException`, usada nas violações de regra de negócio.
- **Entidades**: `Usuario` (herda `IdentityUser<Guid>`), `Disponibilidade` e `Agendamento` (entidade rica: `Cancelar`, `Confirmar`, `Concluir`).
- **Interfaces**: `IAgendamentoRepository`, `IDisponibilidadeRepository`, `IUsuarioRepository`.

### ✅ Fase 3: Infraestrutura (`src/Infrastructure`)
- `Data/Configurations/`: Fluent API das 3 entidades (`DeleteBehavior.Restrict` e índices).
- `Data/ApplicationDbContext.cs`: herda de `IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>`.
- `Repositories/`: `AgendamentoRepository` (query de conflito), `DisponibilidadeRepository` e `UsuarioRepository`.
- `Auth/`: `JwtSettings` (seção `Jwt` do appsettings) e `TokenService` (gera o JWT com o perfil como *role*).
- `DependencyInjection.cs`: método `AddInfrastructure()` (DbContext, Identity, repositórios e JWT).

### ✅ Fase 4: Application (`src/Core/Application`)
- `DTOs/`: requests e responses, escritos como `record`.
- `Validators/`: validadores do FluentValidation (cadastro, login, agendamento, grade).
- `Exceptions/`: `RecursoNaoEncontradoException` (404), `AcessoNegadoException` (403), `CredenciaisInvalidasException` (401).
- `Services/`: `AuthService` (registro e login), `AgendamentoService` (regras 1, 2 e 3) e `DisponibilidadeService` (grade e horários livres).
- `TimeProvider` injetado no lugar de `DateTime.Now`, para os testes controlarem o "agora".
- `DependencyInjection.cs`: método `AddApplication()`.

### ✅ Fase 5: API (`src/Presentation`)
- `Program.cs`: `AddApplication`, `AddInfrastructure`, JWT Bearer, controllers com enums como texto e OpenAPI + Scalar (`/scalar`).
- `Middlewares/GlobalExceptionHandler.cs`: `IExceptionHandler` que converte as exceções em `ProblemDetails`.
- `Controllers/`: `AuthController`, `AgendamentosController` e `DisponibilidadeController`.
- `Extensions/ClaimsPrincipalExtensions.cs`: `User.ObterUsuarioId()` e `User.EhAdmin()`.
- `MinhaAgendaApi.Presentation.http`: exemplos de todas as requisições.

### ✅ Fase 6: Testes (`tests/UnitTests`)
- `Domain/AgendamentoTests.cs`: regra das 2h, fluxo de status e validação do construtor.
- `Application/AgendamentoServiceTests.cs` (com Moq): conflito de horário, fora da grade, data no passado, prestador inexistente e permissão de cancelamento.

---

