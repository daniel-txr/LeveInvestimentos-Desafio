using LeveInvestimentos.Domain.Enums;

namespace LeveInvestimentos.Domain.Entities;

/// <summary>
/// Representa uma tarefa atribuída por um gestor a um usuário subordinado,
/// com uma mensagem descritiva e um prazo (data limite) para execução.
/// </summary>
public class Tarefa : EntidadeBase
{
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataLimite { get; set; }
    public DateTime? DataConclusao { get; set; }
    public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;

    /// <summary>Usuário subordinado responsável por executar a tarefa.</summary>
    public int UsuarioResponsavelId { get; set; }
    public Usuario? UsuarioResponsavel { get; set; }

    /// <summary>Gestor que criou/atribuiu a tarefa.</summary>
    public int UsuarioGestorId { get; set; }
    public Usuario? UsuarioGestor { get; set; }

    /// <summary>Marca a tarefa como concluída e registra a data de conclusão.</summary>
    public void Concluir()
    {
        Status = StatusTarefa.Concluida;
        DataConclusao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public bool Atrasada => Status != StatusTarefa.Concluida && DataLimite.Date < DateTime.UtcNow.Date;
}
