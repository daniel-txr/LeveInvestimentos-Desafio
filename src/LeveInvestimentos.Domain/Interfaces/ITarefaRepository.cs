using LeveInvestimentos.Domain.Entities;

namespace LeveInvestimentos.Domain.Interfaces;

public interface ITarefaRepository : IRepositorioBase<Tarefa>
{
    Task<IEnumerable<Tarefa>> ObterPorGestorAsync(int gestorId);
    Task<IEnumerable<Tarefa>> ObterPorResponsavelAsync(int usuarioResponsavelId);
    Task<Tarefa?> ObterComUsuariosAsync(int tarefaId);
}
