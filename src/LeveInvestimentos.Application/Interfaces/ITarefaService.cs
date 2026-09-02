using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Application.Interfaces;

public interface ITarefaService
{
    Task<Tarefa> CadastrarAsync(TarefaCadastroDTO dto, int gestorId);
    Task<IEnumerable<TarefaListaDTO>> ListarPorGestorAsync(int gestorId);
    Task<IEnumerable<TarefaListaDTO>> ListarPorResponsavelAsync(int usuarioId);
    Task<PaginacaoResultado<TarefaListaDTO>> ListarPorGestorPaginadoAsync(int gestorId, int pagina, int tamanhoPagina);
    Task<PaginacaoResultado<TarefaListaDTO>> ListarPorResponsavelPaginadoAsync(int usuarioId, int pagina, int tamanhoPagina);
    Task ConcluirAsync(int tarefaId, int usuarioLogadoId);

    Task<Tarefa?> ObterPorIdAsync(int tarefaId);
    Task<Tarefa> EditarAsync(TarefaEdicaoDTO dto, int gestorLogadoId);
    Task ExcluirAsync(int tarefaId, int gestorLogadoId);
}
