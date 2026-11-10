using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public TokenGerado GerarToken(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddHours(_settings.ExpiracaoHoras);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Chave));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                // o perfil vira "role" -> usado no [Authorize(Roles = "...")]
                new Claim(ClaimTypes.Role, usuario.TipoPerfil.ToString())
            }),
            Expires = expiraEm,
            Issuer = _settings.Emissor,
            Audience = _settings.Audiencia,
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenGerado(token, expiraEm);
    }
}
