using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Domain.Interfaces;

public interface IDisponibilidadeRepository
{
    Task<IEnumerable<Disponibilidade>> ObterPorPrestadorAsync(Guid prestadorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Disponibilidade>> ObterPrestadorEDiaAsync(Guid prestadorId, DayOfWeek dia, CancellationToken cancellationToken = default);
    Task AdicionarOuAtualizarGradeAsync(Guid prestadorId, IEnumerable<Disponibilidade> disponibilidades, CancellationToken cancellationToken = default);
}