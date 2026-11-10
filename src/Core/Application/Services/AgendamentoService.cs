using FluentValidation;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Exceptions;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;
using MinhaAgendaApi.Domain.Exceptions;
using MinhaAgendaApi.Domain.Interfaces;

namespace MinhaAgendaApi.Application.Services;

public class AgendamentoService : IAgendamentoService
{
    private readonly IAgendamentoRepository _agendamentos;
    private readonly IDisponibilidadeRepository _disponibilidades;
    private readonly IUsuarioRepository _usuarios;
    private readonly IValidator<CriarAgendamentoRequest> _validator;
    private readonly TimeProvider _tempo; // permite "congelar" o relogio nos testes

    public AgendamentoService(
        IAgendamentoRepository agendamentos,
        IDisponibilidadeRepository disponibilidades,
        IUsuarioRepository usuarios,
        IValidator<CriarAgendamentoRequest> validator,
        TimeProvider tempo)
    {
        _agendamentos = agendamentos;
        _disponibilidades = disponibilidades;
        _usuarios = usuarios;
        _validator = validator;
        _tempo = tempo;
    }

    public async Task<AgendamentoResponse> CriarAsync(Guid clienteId, CriarAgendamentoRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (request.DataHoraInicio <= _tempo.GetUtcNow())
            throw new DomainException("O agendamento deve ser feito para uma data futura.");

        var prestador = await _usuarios.ObterPorIdAsync(request.PrestadorId, cancellationToken);
        if (prestador is null || prestador.TipoPerfil != TipoPerfil.Prestador)
            throw new RecursoNaoEncontradoException("Prestador não encontrado.");

        // regra 2: precisa estar dentro da grade de disponibilidade
        await ValidarDentroDaGradeAsync(request, cancellationToken);

        // regra 1: nao pode sobrepor outro agendamento ativo
        var temConflito = await _agendamentos.ExisteConflitoHorarioAsync(
            request.PrestadorId, request.DataHoraInicio, request.DataHoraFim, cancellationToken: cancellationToken);

        if (temConflito)
            throw new DomainException("O prestador já possui um agendamento nesse horário.");

        var agendamento = new Agendamento(clienteId, request.PrestadorId, request.DataHoraInicio, request.DataHoraFim, request.Observacao);
        await _agendamentos.AdicionarAsync(agendamento, cancellationToken);

        return AgendamentoResponse.DeEntidade(agendamento);
    }

    public async Task<IEnumerable<AgendamentoResponse>> ObterMeusAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var lista = await _agendamentos.ObterUsuarioAsync(usuarioId, cancellationToken);
        return lista.Select(AgendamentoResponse.DeEntidade);
    }

    public async Task<AgendamentoResponse> CancelarAsync(Guid agendamentoId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var agendamento = await ObterOuFalharAsync(agendamentoId, cancellationToken);

        if (!ehAdmin && agendamento.ClienteId != usuarioId)
            throw new AcessoNegadoException("Você só pode cancelar os seus próprios agendamentos.");

        // regra 3 (2h de antecedencia) fica dentro da entidade
        agendamento.Cancelar(_tempo.GetUtcNow());
        await _agendamentos.AtualizarAsync(agendamento, cancellationToken);

        return AgendamentoResponse.DeEntidade(agendamento);
    }

    public async Task<AgendamentoResponse> AtualizarStatusAsync(Guid agendamentoId, Guid usuarioId, bool ehAdmin, StatusAgendamento novoStatus, CancellationToken cancellationToken = default)
    {
        var agendamento = await ObterOuFalharAsync(agendamentoId, cancellationToken);

        if (!ehAdmin && agendamento.PrestadorId != usuarioId)
            throw new AcessoNegadoException("Apenas o prestador deste agendamento pode alterar o status.");

        switch (novoStatus)
        {
            case StatusAgendamento.Confirmado:
                agendamento.Confirmar();
                break;
            case StatusAgendamento.Concluido:
                agendamento.Concluir();
                break;
            case StatusAgendamento.Cancelado:
                agendamento.Cancelar(_tempo.GetUtcNow());
                break;
            default:
                throw new DomainException("Status inválido para esta operação.");
        }

        await _agendamentos.AtualizarAsync(agendamento, cancellationToken);
        return AgendamentoResponse.DeEntidade(agendamento);
    }

    private async Task<Agendamento> ObterOuFalharAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _agendamentos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Agendamento não encontrado.");
    }

    private async Task ValidarDentroDaGradeAsync(CriarAgendamentoRequest request, CancellationToken cancellationToken)
    {
        var inicio = request.DataHoraInicio;
        var fim = request.DataHoraFim.ToOffset(inicio.Offset);

        if (fim.Date != inicio.Date)
            throw new DomainException("O agendamento deve começar e terminar no mesmo dia.");

        var grade = await _disponibilidades.ObterPrestadorEDiaAsync(request.PrestadorId, inicio.DayOfWeek, cancellationToken);

        var cabeNaGrade = grade.Any(d => d.HoraInicio <= inicio.TimeOfDay && d.HoraFim >= fim.TimeOfDay);

        if (!cabeNaGrade)
            throw new DomainException("Horário fora da disponibilidade do prestador.");
    }
}
