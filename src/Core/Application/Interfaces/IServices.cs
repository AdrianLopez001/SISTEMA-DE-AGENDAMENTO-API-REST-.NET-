using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;

namespace MinhaAgendaApi.Application.Interfaces;

// implementado na Infrastructure (depende de bibliotecas de JWT)
public interface ITokenService
{
    TokenGerado GerarToken(Usuario usuario);
}

public interface IAuthService
{
    Task<UsuarioResponse> RegistrarAsync(RegistrarRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface IAgendamentoService
{
    Task<AgendamentoResponse> CriarAsync(Guid clienteId, CriarAgendamentoRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<AgendamentoResponse>> ObterMeusAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<AgendamentoResponse> CancelarAsync(Guid agendamentoId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<AgendamentoResponse> AtualizarStatusAsync(Guid agendamentoId, Guid usuarioId, bool ehAdmin, StatusAgendamento novoStatus, CancellationToken cancellationToken = default);
}

public interface IDisponibilidadeService
{
    Task<IEnumerable<DisponibilidadeResponse>> CadastrarGradeAsync(Guid prestadorId, CadastrarGradeRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<DisponibilidadeResponse>> ObterGradeAsync(Guid prestadorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HorarioLivreResponse>> ObterHorariosLivresAsync(Guid prestadorId, DateOnly data, int duracaoMinutos, CancellationToken cancellationToken = default);
}
