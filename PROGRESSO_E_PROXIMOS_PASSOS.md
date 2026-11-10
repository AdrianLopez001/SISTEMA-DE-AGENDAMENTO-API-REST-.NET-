# 📌 Status de Desenvolvimento: Sistema de Agendamento (API REST .NET)

> **Documento de Continuidade:** registra o que foi feito em cada fase e serve de guia de estudo do projeto.

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

## ⚠️ Pendência local (não afeta o GitHub)

A migration `InitialCreate` **não foi gerada nesta máquina**. O Smart App Control do Windows 11 bloqueou o carregamento de `MinhaAgendaApi.Presentation.dll` pelo `dotnet ef` (erro 0x800711C7). Quem clonar o projeto gera a migration pelos comandos do README:

```powershell
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Presentation
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation
```
