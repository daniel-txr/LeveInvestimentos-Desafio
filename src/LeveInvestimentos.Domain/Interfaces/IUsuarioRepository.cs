using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Domain.Interfaces;

public interface IUsuarioRepository : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email, int? idParaIgnorar = null);
    Task<IEnumerable<Usuario>> ObterSubordinadosAsync(int gestorId);
    Task<IEnumerable<Usuario>> ObterGestoresAsync();
    Task<IEnumerable<Usuario>> ObterTodosComGestorAsync();

    /// <summary>Retorna uma página de usuários (com o Gestor já carregado) e o total de registros, para montar a paginação.</summary>
    Task<(IEnumerable<Usuario> Itens, int Total)> ObterPaginadoComGestorAsync(int pagina, int tamanhoPagina);
}
