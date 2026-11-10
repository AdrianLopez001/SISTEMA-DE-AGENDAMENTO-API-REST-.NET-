using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
