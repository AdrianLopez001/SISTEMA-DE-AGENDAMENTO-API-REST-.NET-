using Microsoft.EntityFrameworkCore;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Interfaces;
using MinhaAgendaApi.Infrastructure.Data;

namespace MinhaAgendaApi.Infrastructure.Repositories;

public class DisponibilidadeRepository : IDisponibilidadeRepository
{
    private readonly ApplicationDbContext _context;

    public DisponibilidadeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Disponibilidade>> ObterPorPrestadorAsync(Guid prestadorId, CancellationToken cancellationToken = default)
    {
        return await _context.Disponibilidades
            .AsNoTracking()
            .Where(d => d.PrestadorId == prestadorId)
            .OrderBy(d => d.DiaDaSemana).ThenBy(d => d.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Disponibilidade>> ObterPrestadorEDiaAsync(Guid prestadorId, DayOfWeek dia, CancellationToken cancellationToken = default)
    {
        return await _context.Disponibilidades
            .AsNoTracking()
            .Where(d => d.PrestadorId == prestadorId && d.DiaDaSemana == dia)
            .ToListAsync(cancellationToken);
    }

    // estrategia simples: apaga a grade antiga e grava a nova
    public async Task AdicionarOuAtualizarGradeAsync(Guid prestadorId, IEnumerable<Disponibilidade> disponibilidades, CancellationToken cancellationToken = default)
    {
        var antigas = await _context.Disponibilidades
            .Where(d => d.PrestadorId == prestadorId)
            .ToListAsync(cancellationToken);

        _context.Disponibilidades.RemoveRange(antigas);

        var novas = disponibilidades.ToList();
        foreach (var d in novas)
            d.PrestadorId = prestadorId;

        await _context.Disponibilidades.AddRangeAsync(novas, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
