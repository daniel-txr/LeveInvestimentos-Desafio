using System.ComponentModel.DataAnnotations;
using LeveInvestimentos.Domain.Enums;

namespace LeveInvestimentos.Application.DTOs;

/// <summary>Dados necessários para autenticação de um usuário.</summary>
public class LoginDTO
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Dados de entrada para cadastro de um novo usuário (somente gestores).</summary>
public class UsuarioCadastroDTO
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
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

    /// <summary>
    /// Caminho relativo (dentro de wwwroot) onde a foto já foi salva pela camada Web.
    /// </summary>
    public string? CaminhoFoto { get; set; }
}

/// <summary>Dados de um usuário para exibição em listagens e detalhes.</summary>
public class UsuarioListaDTO
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TelefoneCelular { get; set; } = string.Empty;
    public string? CaminhoFoto { get; set; }
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; }
    public string? NomeGestor { get; set; }
}
