using Microsoft.EntityFrameworkCore;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;
using MinhaAgendaApi.Domain.Interfaces;
using MinhaAgendaApi.Infrastructure.Data;

namespace MinhaAgendaApi.Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly ApplicationDbContext _context;

    public AgendamentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Agendamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Agendamento>> ObterUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Agendamentos
            .AsNoTracking()
            .Where(a => a.ClienteId == usuarioId || a.PrestadorId == usuarioId)
            .OrderBy(a => a.DataHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteConflitoHorarioAsync(Guid prestadorId, DateTimeOffset inicio, DateTimeOffset fim,
        Guid? agendamentoIdIgnorado = null, CancellationToken cancellationToken = default)
    {
        // NovoInicio < ExistenteFim AND NovoFim > ExistenteInicio
        return await _context.Agendamentos.AnyAsync(a =>
            a.PrestadorId == prestadorId &&
            a.Status != StatusAgendamento.Cancelado &&
            (agendamentoIdIgnorado == null || a.Id != agendamentoIdIgnorado) &&
            inicio < a.DataHoraFim &&
            fim > a.DataHoraInicio,
            cancellationToken);
    }

    public async Task AdicionarAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        await _context.Agendamentos.AddAsync(agendamento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        _context.Agendamentos.Update(agendamento);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
