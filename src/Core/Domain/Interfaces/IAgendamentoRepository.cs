using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Domain.Interfaces;

public interface IAgendamentoRepository
{
    Task<Agendamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Agendamento>> ObterUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    //regra de ouro da documentacao: validacao de sobreposicao de horario no banco
    // novoinicio <existentefim and novofim > existenteinicio
    Task<bool> ExisteConflitoHorarioAsync(
        Guid prestadorId,
        DateTimeOffset inicio,
        DateTimeOffset fim,
        Guid? agendamentoIdIgnorado = null,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Agendamento agendamento, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Agendamento agendamento, CancellationToken cancellationToken = default);
}