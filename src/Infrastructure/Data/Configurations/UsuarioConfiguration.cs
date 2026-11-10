using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinhaAgendaApi.Domain.Entities;

namespace MinhaAgendaApi.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.NomeCompleto).IsRequired().HasMaxLength(150);
        builder.Property(u => u.TipoPerfil).IsRequired();
    }
}
