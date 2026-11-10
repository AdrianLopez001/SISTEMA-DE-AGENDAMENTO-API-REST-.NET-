using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Infrastructure.Data.Configurations;

public class DisponibilidadeConfiguration : IEntityTypeConfiguration<Disponibilidade>
{
    public void Configure(EntityTypeBuilder<Disponibilidade> builder)
    {
        builder.ToTable("Disponibilidades");
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.Prestador)
               .WithMany(u => u.Disponibilidades)
               .HasForeignKey(d => d.PrestadorId)
               .OnDelete(DeleteBehavior.Cascade);

        // indice para buscar a grade por prestador e dia
        builder.HasIndex(d => new { d.PrestadorId, d.DiaDaSemana });
    }
}
