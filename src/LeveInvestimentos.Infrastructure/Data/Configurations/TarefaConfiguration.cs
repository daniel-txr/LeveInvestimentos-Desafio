using LeveInvestimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeveInvestimentos.Infrastructure.Data.Configurations;

public class TarefaConfiguration : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("Tarefas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Mensagem)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.DataLimite).HasColumnType("date");

        builder.HasOne(t => t.UsuarioResponsavel)
            .WithMany(u => u.TarefasAtribuidas)
            .HasForeignKey(t => t.UsuarioResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.UsuarioGestor)
            .WithMany(u => u.TarefasCriadas)
            .HasForeignKey(t => t.UsuarioGestorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Acelera as consultas mais frequentes: "minhas tarefas" e "tarefas que criei".
        builder.HasIndex(t => t.UsuarioResponsavelId);
        builder.HasIndex(t => t.UsuarioGestorId);
    }
}
