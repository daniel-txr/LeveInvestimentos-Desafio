using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Domain.Interfaces;

public interface IUsuarioRepository : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email, int? idParaIgnorar = null);
    Task<IEnumerable<Usuario>> ObterSubordinadosAsync(int gestorId);
    Task<IEnumerable<Usuario>> ObterGestoresAsync();
    Task<IEnumerable<Usuario>> ObterTodosComGestorAsync();
}
