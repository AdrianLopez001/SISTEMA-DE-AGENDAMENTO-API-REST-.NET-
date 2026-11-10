namespace MinhaAgendaApi.Infrastructure.Auth;

// espelha a secao "Jwt" do appsettings.json
public class JwtSettings
{
    public const string Secao = "Jwt";

    public string Chave { get; set; } = string.Empty;   // minimo 32 caracteres (HS256)
    public string Emissor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int ExpiracaoHoras { get; set; } = 2;
}
