using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Application.Services;

namespace MinhaAgendaApi.Application;

public static class DependencyInjection
{
    // chamado no Program.cs: builder.Services.AddApplication();
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // registra todos os AbstractValidator<T> deste projeto
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAgendamentoService, AgendamentoService>();
        services.AddScoped<IDisponibilidadeService, DisponibilidadeService>();

        return services;
    }
}
