using System.ComponentModel.DataAnnotations;
using LeveInvestimentos.Domain.Enums;

namespace LeveInvestimentos.Web.Models;

/// <summary>
/// ViewModel usado na tela de cadastro de usuário. Estende os dados de negócio
/// (que ficam no DTO da camada Application) com o IFormFile da foto, que é um
/// detalhe específico de HTTP/Web e por isso não pertence à camada de Application.
/// </summary>
public class UsuarioCadastroViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime DataNascimento { get; set; } = DateTime.Today.AddYears(-25);

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

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
    [Display(Name = "Confirmação de senha")]
    public string ConfirmacaoSenha { get; set; } = string.Empty;

    [Display(Name = "Perfil")]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Subordinado;

    [Display(Name = "Foto do usuário")]
    public IFormFile? Foto { get; set; }
}
