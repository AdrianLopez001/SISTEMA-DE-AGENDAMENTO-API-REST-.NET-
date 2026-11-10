using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;

namespace MinhaAgendaApi.Application.DTOs;

public record CriarAgendamentoRequest(
    Guid PrestadorId,
    DateTimeOffset DataHoraInicio,
    DateTimeOffset DataHoraFim,
    string? Observacao);

public record AtualizarStatusRequest(StatusAgendamento Status);

public record AgendamentoResponse(
    Guid Id,
    Guid ClienteId,
    Guid PrestadorId,
    DateTimeOffset DataHoraInicio,
    DateTimeOffset DataHoraFim,
    StatusAgendamento Status,
    string? Observacao,
    DateTimeOffset CriadoEm)
{
    public static AgendamentoResponse DeEntidade(Agendamento a) =>
        new(a.Id, a.ClienteId, a.PrestadorId, a.DataHoraInicio, a.DataHoraFim, a.Status, a.Observacao, a.CriadoEm);
}
