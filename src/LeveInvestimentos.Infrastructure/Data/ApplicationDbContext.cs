using LeveInvestimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automaticamente todas as classes IEntityTypeConfiguration<T> presentes no assembly,
        // mantendo o mapeamento de cada entidade isolado em seu próprio arquivo (Single Responsibility).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // Recriando automatização do updated_at do Laravel Eloquent
    public override int SaveChanges()
    {
        AtualizarDataDeAtualizacao();
        return base.SaveChanges();
    }

    // Recriando automatização do updated_at do Laravel Eloquent
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarDataDeAtualizacao();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AtualizarDataDeAtualizacao()
    {
        var entradas = ChangeTracker.Entries<EntidadeBase>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entrada in entradas)
            entrada.Entity.DataAtualizacao = DateTime.UtcNow;
    }
}
