using System.Linq.Expressions;
using LeveInvestimentos.Domain.Interfaces;
using LeveInvestimentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Repositories;

/// <summary>
/// Implementação genérica reutilizável por todos os repositórios concretos,
/// evitando repetição de código CRUD básico (DRY).
/// </summary>
public class RepositorioBase<T> : IRepositorioBase<T> where T : class
{
    protected readonly ApplicationDbContext Contexto;
    protected readonly DbSet<T> DbSet;

    public RepositorioBase(ApplicationDbContext contexto)
    {
        Contexto = contexto;
        DbSet = contexto.Set<T>();
    }

    public async Task<T?> ObterPorIdAsync(int id) => await DbSet.FindAsync(id);

    public async Task<IEnumerable<T>> ObterTodosAsync() => await DbSet.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado)
        => await DbSet.AsNoTracking().Where(predicado).ToListAsync();

    public async Task AdicionarAsync(T entidade) => await DbSet.AddAsync(entidade);

    public void Atualizar(T entidade) => DbSet.Update(entidade);

    public void Remover(T entidade) => DbSet.Remove(entidade);

    public async Task<int> SalvarAlteracoesAsync() => await Contexto.SaveChangesAsync();
}
