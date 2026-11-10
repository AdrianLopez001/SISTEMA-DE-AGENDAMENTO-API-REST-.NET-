using FluentValidation;
using MinhaAgendaApi.Application.DTOs;
using MinhaAgendaApi.Domain.Enums;

namespace MinhaAgendaApi.Application.Validators;

public class RegistrarRequestValidator : AbstractValidator<RegistrarRequest>
{
    public RegistrarRequestValidator()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(6);

        // ninguem se cadastra como Admin pela API publica
        RuleFor(x => x.TipoPerfil)
            .Must(t => t == TipoPerfil.Cliente || t == TipoPerfil.Prestador)
            .WithMessage("Perfil deve ser Cliente ou Prestador.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty();
    }
}

public class CriarAgendamentoRequestValidator : AbstractValidator<CriarAgendamentoRequest>
{
    public CriarAgendamentoRequestValidator()
    {
        RuleFor(x => x.PrestadorId).NotEmpty();
        RuleFor(x => x.DataHoraFim)
            .GreaterThan(x => x.DataHoraInicio)
            .WithMessage("A data/hora de término deve ser maior que o início.");
        RuleFor(x => x.Observacao).MaximumLength(500);
    }
}

public class CadastrarGradeRequestValidator : AbstractValidator<CadastrarGradeRequest>
{
    public CadastrarGradeRequestValidator()
    {
        RuleFor(x => x.Itens).NotEmpty().WithMessage("Informe ao menos um horário.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.DiaDaSemana).IsInEnum();
            item.RuleFor(i => i.HoraInicio).GreaterThanOrEqualTo(TimeSpan.Zero);
            item.RuleFor(i => i.HoraFim)
                .GreaterThan(i => i.HoraInicio)
                .LessThanOrEqualTo(TimeSpan.FromHours(24))
                .WithMessage("HoraFim deve ser maior que HoraInicio e no máximo 24:00.");
        });
    }
}
