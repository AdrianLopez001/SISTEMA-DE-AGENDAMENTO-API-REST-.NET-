using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Infrastructure.Data.Configurations;

public class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("Agendamentos");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.Observacao).HasMaxLength(500);

        // Restrict: as duas FKs apontam para Usuario e cascade nas duas gera ciclo no SQL Server
        builder.HasOne(a => a.Cliente)
               .WithMany(u => u.AgendamentosComoCliente)
               .HasForeignKey(a => a.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Prestador)
               .WithMany(u => u.AgendamentosComoPrestador)
               .HasForeignKey(a => a.PrestadorId)
               .OnDelete(DeleteBehavior.Restrict);

        // indice composto para a busca de conflito de horario
        builder.HasIndex(a => new { a.PrestadorId, a.DataHoraInicio, a.DataHoraFim });
    }
}
