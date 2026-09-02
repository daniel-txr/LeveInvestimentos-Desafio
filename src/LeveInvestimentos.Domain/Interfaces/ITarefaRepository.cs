using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Domain.Interfaces;

public interface ITarefaRepository : IRepositorioBase<Tarefa>
{
    Task<IEnumerable<Tarefa>> ObterPorGestorAsync(int gestorId);
    Task<IEnumerable<Tarefa>> ObterPorResponsavelAsync(int usuarioResponsavelId);
    Task<Tarefa?> ObterComUsuariosAsync(int tarefaId);

    Task<(IEnumerable<Tarefa> Itens, int Total)> ObterPorGestorPaginadoAsync(int gestorId, int pagina, int tamanhoPagina);
    Task<(IEnumerable<Tarefa> Itens, int Total)> ObterPorResponsavelPaginadoAsync(int usuarioResponsavelId, int pagina, int tamanhoPagina);
}
