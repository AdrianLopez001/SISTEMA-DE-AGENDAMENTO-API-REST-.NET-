using System.Security.Claims;
using MinhaAgendaApi.Domain.Enums;

namespace MinhaAgendaApi.Presentation.Extensions;

public static class ClaimsPrincipalExtensions
{
    // le o Id do usuario logado de dentro do token JWT
    public static Guid ObterUsuarioId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(valor, out var id)
            ? id
            : throw new UnauthorizedAccessException("Token sem identificação de usuário.");
    }

    public static bool EhAdmin(this ClaimsPrincipal user) => user.IsInRole(nameof(TipoPerfil.Admin));
}
