using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Application.Exceptions;
using MinhaAgendaApi.Application.Services;
using MinhaAgendaApi.Application.Validators;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Enums;
using MinhaAgendaApi.Domain.Exceptions;
using MinhaAgendaApi.Domain.Interfaces;
using Moq;

namespace MinhaAgendaApi.UnitTests.Application;

public class AgendamentoServiceTests
{
    private static readonly TimeSpan Fuso = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset Agora = new(2026, 11, 23, 8, 0, 0, Fuso);

    private readonly Mock<IAgendamentoRepository> _agendamentos = new();
    private readonly Mock<IDisponibilidadeRepository> _disponibilidades = new();
    private readonly Mock<IUsuarioRepository> _usuarios = new();

    private readonly Guid _clienteId = Guid.NewGuid();
    private readonly Guid _prestadorId = Guid.NewGuid();

    public AgendamentoServiceTests()
    {
        // prestador existe
        _usuarios
            .Setup(r => r.ObterPorIdAsync(_prestadorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Usuario { Id = _prestadorId, TipoPerfil = TipoPerfil.Prestador });

        // grade do prestador: 08h as 18h
        _disponibilidades
            .Setup(r => r.ObterPrestadorEDiaAsync(_prestadorId, It.IsAny<DayOfWeek>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Disponibilidade>
            {
                new() { PrestadorId = _prestadorId, HoraInicio = TimeSpan.FromHours(8), HoraFim = TimeSpan.FromHours(18) }
            });
    }

    private AgendamentoService CriarService() =>
        new(_agendamentos.Object, _disponibilidades.Object, _usuarios.Object,
            new CriarAgendamentoRequestValidator(), new TempoFixo(Agora));

    private CriarAgendamentoRequest Requisicao(int horaInicio, int horaFim, Guid? prestadorId = null) =>
        new(prestadorId ?? _prestadorId,
            new DateTimeOffset(2026, 11, 23, horaInicio, 0, 0, Fuso),
            new DateTimeOffset(2026, 11, 23, horaFim, 0, 0, Fuso),
            null);

    private void SimularConflito(bool existe) =>
        _agendamentos
            .Setup(r => r.ExisteConflitoHorarioAsync(_prestadorId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existe);

    [Fact]
    public async Task CriarAsync_SemConflito_DeveSalvarAgendamentoPendente()
    {
        SimularConflito(false);

        var resultado = await CriarService().CriarAsync(_clienteId, Requisicao(10, 11));

        Assert.Equal(StatusAgendamento.Pendente, resultado.Status);
        Assert.Equal(_clienteId, resultado.ClienteId);
        _agendamentos.Verify(r => r.AdicionarAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_ComConflitoDeHorario_DeveLancarDomainException()
    {
        SimularConflito(true);

        await Assert.ThrowsAsync<DomainException>(() => CriarService().CriarAsync(_clienteId, Requisicao(10, 11)));

        _agendamentos.Verify(r => r.AdicionarAsync(It.IsAny<Agendamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ForaDaGradeDoPrestador_DeveLancarDomainException()
    {
        SimularConflito(false);

        // 17h as 19h passa do fim da grade (18h)
        await Assert.ThrowsAsync<DomainException>(() => CriarService().CriarAsync(_clienteId, Requisicao(17, 19)));
    }

    [Fact]
    public async Task CriarAsync_NoPassado_DeveLancarDomainException()
    {
        SimularConflito(false);

        // "agora" e 08h, entao 06h ja passou
        await Assert.ThrowsAsync<DomainException>(() => CriarService().CriarAsync(_clienteId, Requisicao(6, 7)));
    }

    [Fact]
    public async Task CriarAsync_PrestadorInexistente_DeveLancarRecursoNaoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            CriarService().CriarAsync(_clienteId, Requisicao(10, 11, prestadorId: Guid.NewGuid())));
    }

    [Fact]
    public async Task CancelarAsync_AgendamentoDeOutroCliente_DeveLancarAcessoNegado()
    {
        var agendamento = new Agendamento(Guid.NewGuid(), _prestadorId, Agora.AddDays(1), Agora.AddDays(1).AddHours(1));
        _agendamentos.Setup(r => r.ObterPorIdAsync(agendamento.Id, It.IsAny<CancellationToken>())).ReturnsAsync(agendamento);

        await Assert.ThrowsAsync<AcessoNegadoException>(() =>
            CriarService().CancelarAsync(agendamento.Id, _clienteId, ehAdmin: false));
    }

    [Fact]
    public async Task CancelarAsync_DonoComAntecedencia_DeveCancelarESalvar()
    {
        var agendamento = new Agendamento(_clienteId, _prestadorId, Agora.AddDays(1), Agora.AddDays(1).AddHours(1));
        _agendamentos.Setup(r => r.ObterPorIdAsync(agendamento.Id, It.IsAny<CancellationToken>())).ReturnsAsync(agendamento);

        var resultado = await CriarService().CancelarAsync(agendamento.Id, _clienteId, ehAdmin: false);

        Assert.Equal(StatusAgendamento.Cancelado, resultado.Status);
        _agendamentos.Verify(r => r.AtualizarAsync(agendamento, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_AgendamentoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            CriarService().CancelarAsync(Guid.NewGuid(), _clienteId, ehAdmin: false));
    }

    // relogio fixo para os testes nao dependerem da hora atual
    private sealed class TempoFixo : TimeProvider
    {
        private readonly DateTimeOffset _agora;
        public TempoFixo(DateTimeOffset agora) => _agora = agora;
        public override DateTimeOffset GetUtcNow() => _agora.ToUniversalTime();
    }
}
