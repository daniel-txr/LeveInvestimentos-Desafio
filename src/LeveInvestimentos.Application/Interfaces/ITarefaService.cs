using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Application.Interfaces;

public interface ITarefaService
{
    Task<Tarefa> CadastrarAsync(TarefaCadastroDTO dto, int gestorId);
    Task<IEnumerable<TarefaListaDTO>> ListarPorGestorAsync(int gestorId);
    Task<IEnumerable<TarefaListaDTO>> ListarPorResponsavelAsync(int usuarioId);
    Task ConcluirAsync(int tarefaId, int usuarioLogadoId);
}
