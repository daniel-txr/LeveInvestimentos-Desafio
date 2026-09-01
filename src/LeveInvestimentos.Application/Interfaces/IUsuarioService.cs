using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> AutenticarAsync(LoginDTO dto);
    Task<Usuario> CadastrarAsync(UsuarioCadastroDTO dto, int gestorLogadoId);
    Task<IEnumerable<UsuarioListaDTO>> ListarAsync();
    Task<IEnumerable<Usuario>> ListarSubordinadosAsync(int gestorId);
    Task<Usuario?> ObterPorIdAsync(int id);
}
