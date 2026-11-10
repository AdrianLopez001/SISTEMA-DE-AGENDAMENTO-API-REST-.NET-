using Microsoft.EntityFrameworkCore;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Interfaces;
using MinhaAgendaApi.Infrastructure.Data;

namespace MinhaAgendaApi.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
