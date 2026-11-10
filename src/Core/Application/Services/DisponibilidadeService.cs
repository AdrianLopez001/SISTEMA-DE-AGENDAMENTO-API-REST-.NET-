using FluentValidation;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;
using MinhaAgendaApi.Domain.Exceptions;
using MinhaAgendaApi.Domain.Interfaces;

namespace MinhaAgendaApi.Application.Services;

public class DisponibilidadeService : IDisponibilidadeService
{
    private readonly IDisponibilidadeRepository _disponibilidades;
    private readonly IAgendamentoRepository _agendamentos;
    private readonly IValidator<CadastrarGradeRequest> _validator;

    public DisponibilidadeService(
        IDisponibilidadeRepository disponibilidades,
        IAgendamentoRepository agendamentos,
        IValidator<CadastrarGradeRequest> validator)
    {
        _disponibilidades = disponibilidades;
        _agendamentos = agendamentos;
        _validator = validator;
    }

    public async Task<IEnumerable<DisponibilidadeResponse>> CadastrarGradeAsync(Guid prestadorId, CadastrarGradeRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (TemFaixaSobreposta(request.Itens))
            throw new DomainException("Existem faixas de horário sobrepostas no mesmo dia.");

        var grade = request.Itens
            .Select(i => new Disponibilidade
            {
                PrestadorId = prestadorId,
                DiaDaSemana = i.DiaDaSemana,
                HoraInicio = i.HoraInicio,
                HoraFim = i.HoraFim
            })
            .ToList();

        await _disponibilidades.AdicionarOuAtualizarGradeAsync(prestadorId, grade, cancellationToken);

        return grade.Select(DisponibilidadeResponse.DeEntidade);
    }

    public async Task<IEnumerable<DisponibilidadeResponse>> ObterGradeAsync(Guid prestadorId, CancellationToken cancellationToken = default)
    {
        var grade = await _disponibilidades.ObterPorPrestadorAsync(prestadorId, cancellationToken);
        return grade.Select(DisponibilidadeResponse.DeEntidade);
    }

    // quebra a grade do dia em blocos de "duracaoMinutos" e remove os que ja estao ocupados
    public async Task<IEnumerable<HorarioLivreResponse>> ObterHorariosLivresAsync(Guid prestadorId, DateOnly data, int duracaoMinutos, CancellationToken cancellationToken = default)
    {
        if (duracaoMinutos <= 0 || duracaoMinutos > 24 * 60)
            throw new DomainException("A duração deve estar entre 1 e 1440 minutos.");

        var duracao = TimeSpan.FromMinutes(duracaoMinutos);

        var grade = await _disponibilidades.ObterPrestadorEDiaAsync(prestadorId, data.DayOfWeek, cancellationToken);

        var ocupados = (await _agendamentos.ObterUsuarioAsync(prestadorId, cancellationToken))
            .Where(a => a.PrestadorId == prestadorId
                     && a.Status != StatusAgendamento.Cancelado
                     && DateOnly.FromDateTime(a.DataHoraInicio.DateTime) == data)
            .ToList();

        var livres = new List<HorarioLivreResponse>();

        foreach (var faixa in grade.OrderBy(g => g.HoraInicio))
        {
            for (var inicio = faixa.HoraInicio; inicio + duracao <= faixa.HoraFim; inicio += duracao)
            {
                var fim = inicio + duracao;

                var conflita = ocupados.Any(a =>
                    inicio < a.DataHoraFim.TimeOfDay && fim > a.DataHoraInicio.TimeOfDay);

                if (!conflita)
                    livres.Add(new HorarioLivreResponse(inicio, fim));
            }
        }

        return livres;
    }

    private static bool TemFaixaSobreposta(IEnumerable<DisponibilidadeItemRequest> itens)
    {
        foreach (var dia in itens.GroupBy(i => i.DiaDaSemana))
        {
            var ordenados = dia.OrderBy(i => i.HoraInicio).ToList();

            for (var i = 1; i < ordenados.Count; i++)
            {
                if (ordenados[i].HoraInicio < ordenados[i - 1].HoraFim)
                    return true;
            }
        }

        return false;
    }
}
