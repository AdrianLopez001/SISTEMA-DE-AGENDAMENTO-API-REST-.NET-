# 📅 Sistema de Agendamento (API REST .NET)

API RESTful desenvolvida em **.NET 10 / C#** para gestão de agendamentos e horários. Inclui autenticação via **JWT**, controle de acesso por perfil (RBAC), gestão da grade de disponibilidade dos prestadores e prevenção de sobreposição de horários (overbooking).

---

## 🎯 Objetivos do Projeto

* Aplicar **Clean Architecture** com separação clara de responsabilidades.
* Usar **ASP.NET Core Identity** + **JWT** para autenticação e autorização.
* Manter as regras de negócio numa **entidade rica**, cobertas por **testes unitários (xUnit + Moq)**.
* Mapear o banco com **Entity Framework Core** via Fluent API.
* Validar entradas com **FluentValidation** e padronizar erros com **ProblemDetails (RFC 7807)**.

---

## 🧑‍💻 Sobre o desenvolvimento

Projeto de estudo para reforçar o desenvolvimento de APIs REST em uma linguagem nova para mim: **C# com .NET**.

| Eu fiz | A IA auxiliou |
| :--- | :--- |
| Arquitetura e estrutura de pastas e arquivos | Sintaxe da linguagem C# |
| Solução, projetos e referências entre camadas | Implementação de partes da infraestrutura e da API |
| Modelagem do domínio (entidades, enums, interfaces) | Testes unitários (xUnit + Moq) |
| Regras de negócio, requisitos e planejamento em fases | |

O arquivo [PROGRESSO_E_PROXIMOS_PASSOS.md](PROGRESSO_E_PROXIMOS_PASSOS.md) registra o estudo de cada fase.

---

## 🏗️ Arquitetura

```text
MinhaAgendaApi/
├── src/
│   ├── Core/
│   │   ├── Domain/           # Entidades, Enums, Exceções de domínio e interfaces de repositório
│   │   └── Application/      # Services (casos de uso), DTOs, Validadores e interfaces de serviço
│   ├── Infrastructure/       # DbContext, Configurations (Fluent API), Repositórios, Identity e JWT
│   └── Presentation/         # Controllers, tratamento global de erros e Program.cs
└── tests/
    └── UnitTests/            # Testes das regras de negócio (entidade e service)
```

**Dependências entre camadas:**

```text
Presentation ──► Application ──► Domain
      │                ▲
      └──► Infrastructure ┘
```

O `Domain` não depende de nenhuma outra camada. A `Infrastructure` implementa as interfaces que o `Domain` e a `Application` definem.

---

## 🗄️ Modelo de Dados

| Entidade | Campos principais |
| :--- | :--- |
| **Usuario** (herda `IdentityUser<Guid>`) | `NomeCompleto`, `TipoPerfil` (`Cliente`, `Prestador`, `Admin`), `CriadoEm` |
| **Agendamento** | `ClienteId`, `PrestadorId`, `DataHoraInicio`, `DataHoraFim`, `Status` (`Pendente`, `Confirmado`, `Concluido`, `Cancelado`), `Observacao`, `CriadoEm` |
| **Disponibilidade** | `PrestadorId`, `DiaDaSemana`, `HoraInicio`, `HoraFim` |

* As duas FKs de `Agendamento` para `Usuario` usam `DeleteBehavior.Restrict`, porque o SQL Server não aceita múltiplos caminhos de cascata.
* Índice composto `(PrestadorId, DataHoraInicio, DataHoraFim)` para a consulta de conflito de horário.

---

## ⚡ Endpoints

| Método | Rota | Autorização | Descrição |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Público | Cadastro de usuário (Cliente ou Prestador) |
| `POST` | `/api/v1/auth/login` | Público | Autenticação e geração do token JWT |
| `GET` | `/api/v1/disponibilidade?prestadorId=&data=&duracaoMinutos=` | Autenticado | Horários livres de um prestador numa data |
| `GET` | `/api/v1/disponibilidade/{prestadorId}/grade` | Autenticado | Grade semanal do prestador |
| `POST` | `/api/v1/disponibilidade` | Prestador | Cadastra/substitui a grade semanal do prestador logado |
| `POST` | `/api/v1/agendamentos` | Cliente | Cria um novo agendamento |
| `GET` | `/api/v1/agendamentos/meus` | Autenticado | Agendamentos do usuário logado |
| `PATCH` | `/api/v1/agendamentos/{id}/cancelar` | Cliente / Admin | Cancela um agendamento |
| `PATCH` | `/api/v1/agendamentos/{id}/status` | Prestador / Admin | Altera o status (`Confirmado`, `Concluido`, `Cancelado`) |

O arquivo [`src/Presentation/MinhaAgendaApi.Presentation.http`](src/Presentation/MinhaAgendaApi.Presentation.http) tem exemplos prontos de todas as requisições.

### Respostas de erro

Todas as falhas retornam `application/problem+json`:

| Situação | HTTP |
| :--- | :--- |
| Dados inválidos (FluentValidation) | `400` (com a lista de `erros` por campo) |
| Regra de negócio violada (`DomainException`) | `400` |
| Login inválido / sem token | `401` |
| Sem permissão sobre o recurso | `403` |
| Recurso não encontrado | `404` |

---

## 🔒 Regras de Negócio

1. **Conflito de horário:** um agendamento não pode sobrepor outro agendamento ativo (`Status != Cancelado`) do mesmo prestador.
   `NovoInicio < ExistenteFim  AND  NovoFim > ExistenteInicio`
2. **Horário de atendimento:** o agendamento precisa caber inteiro numa faixa da grade de `Disponibilidade` do prestador, no mesmo dia.
3. **Cancelamento:** só com pelo menos **2 horas** de antecedência. Agendamentos concluídos ou já cancelados não podem ser cancelados.
4. **Fluxo de status:** `Pendente → Confirmado → Concluido`. Essa regra fica dentro da entidade `Agendamento`, que tem `Status` com setter privado.
5. **Agendamentos futuros:** não é possível agendar no passado.
6. **Perfis:** pela API pública só é possível se cadastrar como `Cliente` ou `Prestador`.

---

## 🛠️ Tecnologias

* .NET 10 / ASP.NET Core Web API (Controllers)
* Entity Framework Core 10 + SQL Server
* ASP.NET Core Identity + JWT Bearer
* FluentValidation
* OpenAPI + [Scalar](https://github.com/scalar/scalar) (documentação interativa)
* xUnit + Moq

---

## 🚀 Como Executar Localmente

**Pré-requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download) e SQL Server (ou LocalDB).

1. **Clonar o repositório**
   ```bash
   git clone https://github.com/AdrianLopez001/SISTEMA-DE-AGENDAMENTO-API-REST-.NET-.git
   cd SISTEMA-DE-AGENDAMENTO-API-REST-.NET-
   ```

2. **Configurar o banco e o JWT** em `src/Presentation/appsettings.Development.json`:
   * `ConnectionStrings:DefaultConnection` aponta para o LocalDB por padrão.
   * `Jwt:Chave` é uma chave de desenvolvimento, com no mínimo 32 caracteres. **Em produção use uma chave própria via variável de ambiente** (`Jwt__Chave`).

3. **Instalar a ferramenta do EF e criar o banco**
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Presentation
   dotnet ef database update --project src/Infrastructure --startup-project src/Presentation
   ```

4. **Rodar a API**
   ```bash
   dotnet run --project src/Presentation
   ```
   A documentação interativa fica em `http://localhost:5048/scalar`.

5. **Rodar os testes**
   ```bash
   dotnet test
   ```

---

## 📋 Checklist de Desenvolvimento

- [x] **Fase 1: Setup da Solução**: projetos em Clean Architecture e pacotes NuGet
- [x] **Fase 2: Domínio**: entidades, enums, exceções e interfaces de repositório
- [x] **Fase 3: Infraestrutura**: `ApplicationDbContext`, Fluent API, repositórios, Identity e JWT
- [x] **Fase 4: Application**: DTOs, validadores (FluentValidation) e services
- [x] **Fase 5: API**: controllers REST, tratamento global de exceções e OpenAPI/Scalar
- [x] **Fase 6: Testes**: 20 testes unitários (regra de 2h, conflito, grade, permissões)

### 💡 Próximas melhorias

- [ ] Paginação em `GET /agendamentos/meus`
- [ ] Testes de integração com `WebApplicationFactory`
- [ ] Docker Compose (API + SQL Server)
- [ ] Endpoint de administração para criar usuários `Admin`
