using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinhaAgendaApi.Application.Interfaces;
using MinhaAgendaApi.Domain.Entities;
using MinhaAgendaApi.Domain.Interfaces;
using MinhaAgendaApi.Infrastructure.Auth;
using MinhaAgendaApi.Infrastructure.Data;
using MinhaAgendaApi.Infrastructure.Repositories;

namespace MinhaAgendaApi.Infrastructure;

public static class DependencyInjection
{
    // chamado no Program.cs: builder.Services.AddInfrastructure(builder.Configuration);
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // banco de dados
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ASP.NET Core Identity (hash de senha, usuarios)
        services.AddIdentityCore<Usuario>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // repositorios
        services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
        services.AddScoped<IDisponibilidadeRepository, DisponibilidadeRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Secao));
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
