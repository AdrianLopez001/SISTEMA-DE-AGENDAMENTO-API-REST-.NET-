using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Presentation.Extensions;

namespace MinhaAgendaApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/disponibilidade")]
[Authorize]
public class DisponibilidadeController : ControllerBase
{
    private readonly IDisponibilidadeService _disponibilidadeService;

    public DisponibilidadeController(IDisponibilidadeService disponibilidadeService)
    {
        _disponibilidadeService = disponibilidadeService;
    }

    /// <summary>Consulta os horários livres de um prestador em uma data.</summary>
    /// <example>GET /api/v1/disponibilidade?prestadorId=...&amp;data=2026-11-20&amp;duracaoMinutos=60</example>
    [HttpGet]
    [ProducesResponseType<IEnumerable<HorarioLivreResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> HorariosLivres(
        [FromQuery] Guid prestadorId,
        [FromQuery] DateOnly data,
        [FromQuery] int duracaoMinutos = 60,
        CancellationToken cancellationToken = default)
    {
        var livres = await _disponibilidadeService.ObterHorariosLivresAsync(prestadorId, data, duracaoMinutos, cancellationToken);
        return Ok(livres);
    }

    /// <summary>Retorna a grade semanal cadastrada de um prestador.</summary>
    [HttpGet("{prestadorId:guid}/grade")]
    [ProducesResponseType<IEnumerable<DisponibilidadeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Grade(Guid prestadorId, CancellationToken cancellationToken)
    {
        var grade = await _disponibilidadeService.ObterGradeAsync(prestadorId, cancellationToken);
        return Ok(grade);
    }

    /// <summary>Cadastra (ou substitui) a grade semanal do prestador logado.</summary>
    [HttpPost]
    [Authorize(Roles = "Prestador")]
    [ProducesResponseType<IEnumerable<DisponibilidadeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CadastrarGrade(CadastrarGradeRequest request, CancellationToken cancellationToken)
    {
        var grade = await _disponibilidadeService.CadastrarGradeAsync(User.ObterUsuarioId(), request, cancellationToken);
        return Ok(grade);
    }
}
