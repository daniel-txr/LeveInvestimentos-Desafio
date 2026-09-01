using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Interfaces;
using LeveInvestimentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Repositories;

public class TarefaRepository : RepositorioBase<Tarefa>, ITarefaRepository
{
    public TarefaRepository(ApplicationDbContext contexto) : base(contexto) { }

    public async Task<IEnumerable<Tarefa>> ObterPorGestorAsync(int gestorId)
        => await DbSet.AsNoTracking()
            .Include(t => t.UsuarioResponsavel)
            .Include(t => t.UsuarioGestor)
            .Where(t => t.UsuarioGestorId == gestorId)
            .ToListAsync();

    public async Task<IEnumerable<Tarefa>> ObterPorResponsavelAsync(int usuarioResponsavelId)
        => await DbSet.AsNoTracking()
            .Include(t => t.UsuarioResponsavel)
            .Include(t => t.UsuarioGestor)
            .Where(t => t.UsuarioResponsavelId == usuarioResponsavelId)
            .ToListAsync();

    public async Task<Tarefa?> ObterComUsuariosAsync(int tarefaId)
        => await DbSet
            .Include(t => t.UsuarioResponsavel)
            .Include(t => t.UsuarioGestor)
            .FirstOrDefaultAsync(t => t.Id == tarefaId);
}
