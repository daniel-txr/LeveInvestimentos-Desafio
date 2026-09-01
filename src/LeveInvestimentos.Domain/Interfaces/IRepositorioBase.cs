using System.Linq.Expressions;

namespace LeveInvestimentos.Domain.Interfaces;

/// <summary>
/// Contrato genérico de repositório, reutilizável por qualquer entidade do domínio.
/// Mantém a camada de acesso a dados desacoplada da camada de aplicação (Dependency Inversion).
/// </summary>
public interface IRepositorioBase<T> where T : class
{
    Task<T?> ObterPorIdAsync(int id);
    Task<IEnumerable<T>> ObterTodosAsync();
    Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado);
    Task AdicionarAsync(T entidade);
    void Atualizar(T entidade);
    void Remover(T entidade);
    Task<int> SalvarAlteracoesAsync();
}
