using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Presentation.Extensions;

namespace MinhaAgendaApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/agendamentos")]
[Authorize]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentoService _agendamentoService;

    public AgendamentosController(IAgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
    }

    /// <summary>Cria um novo agendamento para o cliente logado.</summary>
    [HttpPost]
    [Authorize(Roles = "Cliente")]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(CriarAgendamentoRequest request, CancellationToken cancellationToken)
    {
        var agendamento = await _agendamentoService.CriarAsync(User.ObterUsuarioId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, agendamento);
    }

    /// <summary>Lista os agendamentos do usuário logado (como cliente ou prestador).</summary>
    [HttpGet("meus")]
    [ProducesResponseType<IEnumerable<AgendamentoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Meus(CancellationToken cancellationToken)
    {
        var lista = await _agendamentoService.ObterMeusAsync(User.ObterUsuarioId(), cancellationToken);
        return Ok(lista);
    }

    /// <summary>Cancela um agendamento (mínimo de 2 horas de antecedência).</summary>
    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Cliente,Admin")]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var agendamento = await _agendamentoService.CancelarAsync(id, User.ObterUsuarioId(), User.EhAdmin(), cancellationToken);
        return Ok(agendamento);
    }

    /// <summary>Altera o status do atendimento (Confirmado, Concluido ou Cancelado).</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Prestador,Admin")]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AtualizarStatus(Guid id, AtualizarStatusRequest request, CancellationToken cancellationToken)
    {
        var agendamento = await _agendamentoService.AtualizarStatusAsync(
            id, User.ObterUsuarioId(), User.EhAdmin(), request.Status, cancellationToken);
        return Ok(agendamento);
    }
}
