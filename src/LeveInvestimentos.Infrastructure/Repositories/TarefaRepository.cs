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

    public async Task<(IEnumerable<Tarefa> Itens, int Total)> ObterPorGestorPaginadoAsync(int gestorId, int pagina, int tamanhoPagina)
    {
        var consultaBase = DbSet.AsNoTracking().Where(t => t.UsuarioGestorId == gestorId);

        var total = await consultaBase.CountAsync();

        var itens = await consultaBase
            .Include(t => t.UsuarioResponsavel)
            .Include(t => t.UsuarioGestor)
            .OrderBy(t => t.DataLimite)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }

    public async Task<(IEnumerable<Tarefa> Itens, int Total)> ObterPorResponsavelPaginadoAsync(int usuarioResponsavelId, int pagina, int tamanhoPagina)
    {
        var consultaBase = DbSet.AsNoTracking().Where(t => t.UsuarioResponsavelId == usuarioResponsavelId);

        var total = await consultaBase.CountAsync();

        var itens = await consultaBase
            .Include(t => t.UsuarioResponsavel)
            .Include(t => t.UsuarioGestor)
            .OrderBy(t => t.DataLimite)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }
}
