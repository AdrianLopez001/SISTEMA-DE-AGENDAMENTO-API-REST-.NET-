using FluentValidation;
using Microsoft.AspNetCore.Identity;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Exceptions;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Exceptions;

namespace MinhaAgendaApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegistrarRequest> _registrarValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        UserManager<Usuario> userManager,
        ITokenService tokenService,
        IValidator<RegistrarRequest> registrarValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _registrarValidator = registrarValidator;
        _loginValidator = loginValidator;
    }

    public async Task<UsuarioResponse> RegistrarAsync(RegistrarRequest request, CancellationToken cancellationToken = default)
    {
        await _registrarValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new DomainException("Já existe um usuário cadastrado com este e-mail.");

        var usuario = new Usuario
        {
            UserName = request.Email,
            Email = request.Email,
            NomeCompleto = request.NomeCompleto,
            TipoPerfil = request.TipoPerfil
        };

        // o Identity cuida do hash da senha
        var resultado = await _userManager.CreateAsync(usuario, request.Senha);

        if (!resultado.Succeeded)
            throw new DomainException(string.Join(" ", resultado.Errors.Select(e => e.Description)));

        return UsuarioResponse.DeEntidade(usuario);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var usuario = await _userManager.FindByEmailAsync(request.Email);

        if (usuario is null || !await _userManager.CheckPasswordAsync(usuario, request.Senha))
            throw new CredenciaisInvalidasException();

        var token = _tokenService.GerarToken(usuario);

        return new AuthResponse(token.Token, token.ExpiraEm, UsuarioResponse.DeEntidade(usuario));
    }
}
