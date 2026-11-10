using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;

namespace MinhaAgendaApi.Application.DTOs;

public record RegistrarRequest(string NomeCompleto, string Email, string Senha, TipoPerfil TipoPerfil);

public record LoginRequest(string Email, string Senha);

public record UsuarioResponse(Guid Id, string NomeCompleto, string Email, TipoPerfil TipoPerfil)
{
    public static UsuarioResponse DeEntidade(Usuario usuario) =>
        new(usuario.Id, usuario.NomeCompleto, usuario.Email ?? string.Empty, usuario.TipoPerfil);
}

public record AuthResponse(string Token, DateTime ExpiraEm, UsuarioResponse Usuario);

// resultado do servico de token (implementado na Infrastructure)
public record TokenGerado(string Token, DateTime ExpiraEm);
