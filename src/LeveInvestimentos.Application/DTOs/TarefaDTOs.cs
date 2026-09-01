using System.ComponentModel.DataAnnotations;
using LeveInvestimentos.Domain.Enums;

namespace LeveInvestimentos.Application.DTOs;

/// <summary>Dados de entrada para o gestor cadastrar uma nova tarefa a um subordinado.</summary>
public class TarefaCadastroDTO
{
    [Required(ErrorMessage = "Selecione o responsável pela tarefa.")]
    [Display(Name = "Subordinado responsável")]
    public int UsuarioResponsavelId { get; set; }

    [Required(ErrorMessage = "Informe a mensagem descritiva da tarefa.")]
    [StringLength(1000, ErrorMessage = "A mensagem deve ter no máximo {1} caracteres.")]
    [Display(Name = "Mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data limite.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data limite")]
    public DateTime DataLimite { get; set; }
}

/// <summary>Dados de uma tarefa para exibição em listagens de acompanhamento.</summary>
public class TarefaListaDTO
{
    public int Id { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataLimite { get; set; }
    public DateTime? DataConclusao { get; set; }
    public StatusTarefa Status { get; set; }
    public bool Atrasada { get; set; }
    public string UsuarioResponsavelNome { get; set; } = string.Empty;
    public string UsuarioGestorNome { get; set; } = string.Empty;
}
