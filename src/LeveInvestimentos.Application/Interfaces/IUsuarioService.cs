using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> AutenticarAsync(LoginDTO dto);
    Task<Usuario> CadastrarAsync(UsuarioCadastroDTO dto, int gestorLogadoId);
    Task<IEnumerable<UsuarioListaDTO>> ListarAsync();
    Task<PaginacaoResultado<UsuarioListaDTO>> ListarPaginadoAsync(int pagina, int tamanhoPagina);
    Task<IEnumerable<Usuario>> ListarSubordinadosAsync(int gestorId);
    Task<IEnumerable<Usuario>> ListarGestoresAsync(int? idParaExcluir = null);
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario> EditarAsync(UsuarioEdicaoDTO dto, int gestorLogadoId);
    Task AlterarStatusAsync(int usuarioId, bool ativo, int gestorLogadoId);
}
