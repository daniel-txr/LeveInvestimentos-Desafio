using LeveInvestimentos.Domain.Enums;

namespace LeveInvestimentos.Domain.Entities;

/// <summary>
/// Representa um usuário do sistema. Um usuário pode ser Gestor (perfil com permissão
/// para cadastrar novos usuários e atribuir tarefas) ou Subordinado (colaborador comum).
/// A relação de subordinação é feita através de GestorId.
/// </summary>
public class Usuario : EntidadeBase
{
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? TelefoneFixo { get; set; }
    public string TelefoneCelular { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string? CaminhoFoto { get; set; }

    /// <summary>Hash da senha (nunca armazenamos senha em texto puro).</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Perfil do usuário: somente gestores podem cadastrar outros usuários e tarefas.</summary>
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Subordinado;

    public bool Ativo { get; set; } = true;

    /// <summary>Gestor imediato deste usuário (null quando o próprio usuário é gestor).</summary>
    public int? GestorId { get; set; }
    public Usuario? Gestor { get; set; }

    public ICollection<Usuario> Subordinados { get; set; } = new List<Usuario>();
    public ICollection<Tarefa> TarefasAtribuidas { get; set; } = new List<Tarefa>();
    public ICollection<Tarefa> TarefasCriadas { get; set; } = new List<Tarefa>();

    public bool IsGestor => Perfil == PerfilUsuario.Gestor;
}
