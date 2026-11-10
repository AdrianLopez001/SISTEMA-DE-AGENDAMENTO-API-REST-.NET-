using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Application.DTOs;

// horarios no formato "08:00:00"
public record DisponibilidadeItemRequest(DayOfWeek DiaDaSemana, TimeSpan HoraInicio, TimeSpan HoraFim);

public record CadastrarGradeRequest(List<DisponibilidadeItemRequest> Itens);

public record DisponibilidadeResponse(Guid Id, DayOfWeek DiaDaSemana, TimeSpan HoraInicio, TimeSpan HoraFim)
{
    public static DisponibilidadeResponse DeEntidade(Disponibilidade d) =>
        new(d.Id, d.DiaDaSemana, d.HoraInicio, d.HoraFim);
}

public record HorarioLivreResponse(TimeSpan Inicio, TimeSpan Fim);
