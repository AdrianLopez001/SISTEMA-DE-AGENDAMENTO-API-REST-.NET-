using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;
using MinhaAgendaApi.Domain.Exceptions;

namespace MinhaAgendaApi.UnitTests.Domain;

public class AgendamentoTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 11, 23, 14, 0, 0, TimeSpan.FromHours(-3));

    private static Agendamento CriarAgendamento() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Inicio, Inicio.AddHours(1));

    [Fact]
    public void Construtor_ComFimAntesDoInicio_DeveLancarDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Agendamento(Guid.NewGuid(), Guid.NewGuid(), Inicio, Inicio.AddMinutes(-30)));
    }

    [Fact]
    public void NovoAgendamento_DeveComecarPendente()
    {
        var agendamento = CriarAgendamento();

        Assert.Equal(StatusAgendamento.Pendente, agendamento.Status);
    }

    [Theory]
    [InlineData(2)]   // exatamente 2h antes: permitido
    [InlineData(5)]
    [InlineData(48)]
    public void Cancelar_ComAntecedenciaMinimaDeDuasHoras_DeveCancelar(int horasAntes)
    {
        var agendamento = CriarAgendamento();

        agendamento.Cancelar(Inicio.AddHours(-horasAntes));

        Assert.Equal(StatusAgendamento.Cancelado, agendamento.Status);
    }

    [Theory]
    [InlineData(119)] // 1h59 antes
    [InlineData(30)]
    [InlineData(0)]
    public void Cancelar_ComMenosDeDuasHoras_DeveLancarDomainException(int minutosAntes)
    {
        var agendamento = CriarAgendamento();

        Assert.Throws<DomainException>(() => agendamento.Cancelar(Inicio.AddMinutes(-minutosAntes)));
        Assert.Equal(StatusAgendamento.Pendente, agendamento.Status);
    }

    [Fact]
    public void Cancelar_AgendamentoJaCancelado_DeveLancarDomainException()
    {
        var agendamento = CriarAgendamento();
        agendamento.Cancelar(Inicio.AddDays(-1));

        Assert.Throws<DomainException>(() => agendamento.Cancelar(Inicio.AddDays(-1)));
    }

    [Fact]
    public void Cancelar_AgendamentoConcluido_DeveLancarDomainException()
    {
        var agendamento = CriarAgendamento();
        agendamento.Confirmar();
        agendamento.Concluir();

        Assert.Throws<DomainException>(() => agendamento.Cancelar(Inicio.AddDays(-1)));
    }

    [Fact]
    public void Confirmar_E_Concluir_DevemSeguirOFluxoCorreto()
    {
        var agendamento = CriarAgendamento();

        agendamento.Confirmar();
        Assert.Equal(StatusAgendamento.Confirmado, agendamento.Status);

        agendamento.Concluir();
        Assert.Equal(StatusAgendamento.Concluido, agendamento.Status);
    }

    [Fact]
    public void Concluir_SemConfirmar_DeveLancarDomainException()
    {
        var agendamento = CriarAgendamento();

        Assert.Throws<DomainException>(() => agendamento.Concluir());
    }
}
