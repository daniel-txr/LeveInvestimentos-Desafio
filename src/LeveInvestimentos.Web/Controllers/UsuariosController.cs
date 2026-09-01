using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Web.Authorization;
using LeveInvestimentos.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

/// <summary>
/// Cadastro e listagem de usuários. Toda a controller exige perfil de Gestor.
/// </summary>
[Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
public class UsuariosController : Controller
{
    private const long TamanhoMaximoFotoEmBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png" };

    private readonly IUsuarioService _usuarioService;
    private readonly IWebHostEnvironment _ambiente;

    public UsuariosController(IUsuarioService usuarioService, IWebHostEnvironment ambiente)
    {
        _usuarioService = usuarioService;
        _ambiente = ambiente;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _usuarioService.ListarAsync();
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Create() => View(new UsuarioCadastroViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCadastroViewModel viewModel)
    {
        if (viewModel.Foto is not null && !ArquivoValido(viewModel.Foto, out var erroArquivo))
            ModelState.AddModelError(nameof(viewModel.Foto), erroArquivo);

        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            var gestorLogadoId = int.Parse(User.FindFirst(ClaimsPersonalizados.UsuarioId)!.Value);

            string? caminhoFoto = null;
            if (viewModel.Foto is not null)
                caminhoFoto = await SalvarFotoAsync(viewModel.Foto);

            var dto = new UsuarioCadastroDTO
            {
                NomeCompleto = viewModel.NomeCompleto,
                DataNascimento = viewModel.DataNascimento,
                TelefoneFixo = viewModel.TelefoneFixo,
                TelefoneCelular = viewModel.TelefoneCelular,
                Email = viewModel.Email,
                Endereco = viewModel.Endereco,
                Senha = viewModel.Senha,
                ConfirmacaoSenha = viewModel.ConfirmacaoSenha,
                Perfil = viewModel.Perfil,
                CaminhoFoto = caminhoFoto
            };

            await _usuarioService.CadastrarAsync(dto, gestorLogadoId);

            TempData["MensagemSucesso"] = "Usuário cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario is null)
            return NotFound();

        return View(usuario);
    }

    private bool ArquivoValido(IFormFile foto, out string mensagemErro)
    {
        var extensao = Path.GetExtension(foto.FileName).ToLowerInvariant();

        if (!ExtensoesPermitidas.Contains(extensao))
        {
            mensagemErro = "Formato de imagem não suportado. Utilize JPG, PNG ou GIF.";
            return false;
        }

        if (foto.Length > TamanhoMaximoFotoEmBytes)
        {
            mensagemErro = "A foto deve ter no máximo 5 MB.";
            return false;
        }

        mensagemErro = string.Empty;
        return true;
    }

    /// <summary>Salva a foto em wwwroot/uploads/usuarios com nome único e retorna o caminho relativo.</summary>
    private async Task<string> SalvarFotoAsync(IFormFile foto)
    {
        var pastaDestino = Path.Combine(_ambiente.WebRootPath, "uploads", "usuarios");
        Directory.CreateDirectory(pastaDestino);

        var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
        var caminhoCompleto = Path.Combine(pastaDestino, nomeArquivo);

        await using var stream = new FileStream(caminhoCompleto, FileMode.Create);
        await foto.CopyToAsync(stream);

        return $"/uploads/usuarios/{nomeArquivo}";
    }
}
