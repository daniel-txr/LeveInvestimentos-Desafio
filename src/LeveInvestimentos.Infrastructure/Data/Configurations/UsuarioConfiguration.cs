using LeveInvestimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeveInvestimentos.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NomeCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.TelefoneCelular)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.TelefoneFixo)
            .HasMaxLength(20);

        builder.Property(u => u.Endereco)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(u => u.CaminhoFoto)
            .HasMaxLength(500);

        builder.Property(u => u.SenhaHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Perfil)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.DataNascimento).HasColumnType("date");

        builder.HasOne(u => u.Gestor)
            .WithMany(u => u.Subordinados)
            .HasForeignKey(u => u.GestorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
