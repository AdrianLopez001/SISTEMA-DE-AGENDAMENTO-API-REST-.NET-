using Microsoft.AspNetCore.Identity; //chamado do framework
using MinhaAgendaApi.Domain.Enums; // importa os Enums declarados

namespace MinhaAgendaApi.Domain.Entities; // boas praticas =)

public class Usuario : IdentityUser<Guid>
{
    public string NomeCompleto { get; set; } = string.Empty;
    public TipoPerfil TipoPerfil { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    //prioridades de navegação - relacionamentos
    public ICollection<Agendamento> AgendamentosComoCliente { get; set; } = new List<Agendamento>();
    public ICollection<Agendamento> AgendamentosComoPrestador { get; set; } = new List<Agendamento>();
    public ICollection<Disponibilidade> Disponibilidades { get; set; } = new List<Disponibilidade>();
}