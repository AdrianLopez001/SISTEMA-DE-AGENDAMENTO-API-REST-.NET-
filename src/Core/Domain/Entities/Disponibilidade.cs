namespace MinhaAgendaApi.Domain.Entities;

public class Disponibilidade
{
    public Guid Id { get; set; } = Guid.NewGuid();
    //chave estrangeira e propriedade de navegacao para prestador
    public Guid PrestadorId { get; set; }
    public Usuario? Prestador { get; set; }
    // dayofweek enum nativo do .net (sunday 0 ; monday 1...)  
    public DayOfWeek DiaDaSemana { get; set; }
    //timespan guarda apenas o horario ex 08:00:00
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
}