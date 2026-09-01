namespace LeveInvestimentos.Domain.Entities;

/// <summary>
/// Campos de auditoria comuns a todas as entidades do domínio.
/// Centralizar isso aqui evita duplicação e garante consistência
/// em futuras entidades que a equipe venha a criar.
/// </summary>
public abstract class EntidadeBase
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
}
