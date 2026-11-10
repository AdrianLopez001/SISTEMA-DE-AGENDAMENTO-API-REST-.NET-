using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Interfaces;

namespace MinhaAgendaApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Cadastra um novo usuário (Cliente ou Prestador).</summary>
    [HttpPost("register")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(RegistrarRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _authService.RegistrarAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    /// <summary>Autentica o usuário e devolve o token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resposta = await _authService.LoginAsync(request, cancellationToken);
        return Ok(resposta);
    }
}
