using System.ComponentModel.DataAnnotations;
using LeveInvestimentos.Domain.Enums;

namespace LeveInvestimentos.Web.Models;

/// <summary>
/// ViewModel da tela de edição de usuário. Sem campos de senha (troca de senha é um
/// fluxo à parte, fora do escopo desta entrega) e com a foto sendo opcional —
/// se nada for enviado, a foto atual é mantida (ver UsuariosController.Edit).
/// </summary>
public class UsuarioEdicaoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime DataNascimento { get; set; }

    [Phone(ErrorMessage = "Telefone fixo inválido.")]
    [Display(Name = "Telefone fixo")]
    public string? TelefoneFixo { get; set; }

    [Required(ErrorMessage = "Informe o telefone celular.")]
    [Phone(ErrorMessage = "Telefone celular inválido.")]
    [Display(Name = "Telefone celular")]
    public string TelefoneCelular { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereço.")]
    [StringLength(300)]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

    [Display(Name = "Perfil")]
    public PerfilUsuario Perfil { get; set; }

    [Display(Name = "Gestor responsável")]
    public int? GestorId { get; set; }

    [Display(Name = "Nova foto (opcional)")]
    public IFormFile? Foto { get; set; }

    /// <summary>Caminho da foto atual, exibida como preview e mantida caso nenhuma nova seja enviada.</summary>
    public string? CaminhoFotoAtual { get; set; }
}
