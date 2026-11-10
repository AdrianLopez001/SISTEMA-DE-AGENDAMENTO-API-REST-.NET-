using MinhaAgendaApi.Domain.Enums;
using MinhaAgendaApi.Domain.Exceptions;

namespace MinhaAgendaApi.Domain.Entities;

public class Agendamento
{
    public Guid Id { get; set;} = Guid.NewGuid();
    
    //relacionamento com o cliente
    public Guid ClienteId { get; set; }
    public Usuario? Cliente { get; set; }

    //relacionamento com o prestador
    public Guid PrestadorId { get; set; }
    public Usuario? Prestador { get; set; }

    public DateTimeOffset DataHoraInicio { get; set; }
    public DateTimeOffset DataHoraFim { get; set; }
    public StatusAgendamento Status { get; private set; } = StatusAgendamento.Pendente;
    public string? Observacao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;

    //construtor vazio exigido pelo EF core
    public Agendamento() { }

    // construtor de conveciencia para criar um novo agendamento validado
    public Agendamento(Guid clienteId, Guid prestadorId, DateTimeOffset inicio, DateTimeOffset fim, string? observacao = null)
    {
        if (fim <= inicio)
            throw new DomainException("A data/hora de término deve ser maior que o início.");

        ClienteId = clienteId;
        PrestadorId = prestadorId;
        DataHoraInicio = inicio; 
        DataHoraFim = fim;
        Observacao = observacao;
        Status = StatusAgendamento.Pendente;
    }

    // regra de negocio cancelamento com no minimo duas horas de antecedencia
    public void Cancelar(DateTimeOffset dataHoraAtual)
    {
        if (Status == StatusAgendamento.Cancelado)
            throw new DomainException("Este agendamento ja esta cancelado.");
        
        if (Status == StatusAgendamento.Concluido)
            throw new DomainException("Não é possivel cancelar um agendamento ja concluido.");

        if (DataHoraInicio - dataHoraAtual < TimeSpan.FromHours(2))
            throw new DomainException("Agendamentos só podem ser cancelados com no minimo 2 horas de antecedência.");

        Status = StatusAgendamento.Cancelado;
    }

    public void Confirmar()
    {
        if (Status != StatusAgendamento.Pendente)
            throw new DomainException("Apenas agendamentos pendentes podem ser confirmados.");

        Status = StatusAgendamento.Confirmado;
    }

    public void Concluir()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new DomainException("Apenas agendamentos confirmados podem ser concluídos.");

        Status = StatusAgendamento.Concluido;
    }

}
