using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Web.Authorization;
using LeveInvestimentos.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

/// <summary>
/// Cadastro, edição, inativação e listagem de usuários. Toda a controller exige perfil
/// de Gestor, pois somente gestores podem gerenciar usuários no sistema.
/// </summary>
[Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
public class UsuariosController : Controller
{
    private const int TamanhoPagina = 10;
    private const long TamanhoMaximoFotoEmBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png" };

    private readonly IUsuarioService _usuarioService;
    private readonly IWebHostEnvironment _ambiente;

    public UsuariosController(IUsuarioService usuarioService, IWebHostEnvironment ambiente)
    {
        _usuarioService = usuarioService;
        _ambiente = ambiente;
    }

    public async Task<IActionResult> Index(int pagina = 1)
    {
        var resultado = await _usuarioService.ListarPaginadoAsync(pagina, TamanhoPagina);
        return View(resultado);
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
            var gestorLogadoId = ObterUsuarioLogadoId();

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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario is null)
            return NotFound();

        await CarregarGestoresDisponiveis(idParaExcluir: id);

        return View(new UsuarioEdicaoViewModel
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            DataNascimento = usuario.DataNascimento,
            TelefoneFixo = usuario.TelefoneFixo,
            TelefoneCelular = usuario.TelefoneCelular,
            Email = usuario.Email,
            Endereco = usuario.Endereco,
            Perfil = usuario.Perfil,
            GestorId = usuario.GestorId,
            CaminhoFotoAtual = usuario.CaminhoFoto
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UsuarioEdicaoViewModel viewModel)
    {
        if (viewModel.Foto is not null && !ArquivoValido(viewModel.Foto, out var erroArquivo))
            ModelState.AddModelError(nameof(viewModel.Foto), erroArquivo);

        if (!ModelState.IsValid)
        {
            await CarregarGestoresDisponiveis(idParaExcluir: viewModel.Id);
            return View(viewModel);
        }

        try
        {
            var gestorLogadoId = ObterUsuarioLogadoId();

            string? novoCaminhoFoto = null;
            if (viewModel.Foto is not null)
                novoCaminhoFoto = await SalvarFotoAsync(viewModel.Foto);

            var dto = new UsuarioEdicaoDTO
            {
                Id = viewModel.Id,
                NomeCompleto = viewModel.NomeCompleto,
                DataNascimento = viewModel.DataNascimento,
                TelefoneFixo = viewModel.TelefoneFixo,
                TelefoneCelular = viewModel.TelefoneCelular,
                Email = viewModel.Email,
                Endereco = viewModel.Endereco,
                Perfil = viewModel.Perfil,
                GestorId = viewModel.GestorId,
                // null = mantém a foto atual (ver UsuarioService.EditarAsync)
                CaminhoFoto = novoCaminhoFoto
            };

            await _usuarioService.EditarAsync(dto, gestorLogadoId);

            TempData["MensagemSucesso"] = "Usuário atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CarregarGestoresDisponiveis(idParaExcluir: viewModel.Id);
            return View(viewModel);
        }
    }

    /// <summary>
    /// "Excluir" um usuário, na prática, inativa o cadastro (ver UsuarioService.AlterarStatusAsync) —
    /// evita quebrar o histórico de tarefas já vinculado a ele.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, int pagina = 1)
    {
        try
        {
            await _usuarioService.AlterarStatusAsync(id, ativo: false, ObterUsuarioLogadoId());
            TempData["MensagemSucesso"] = "Usuário inativado com sucesso.";
        }
        catch (DominioException ex)
        {
            TempData["MensagemErro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, int pagina = 1)
    {
        try
        {
            await _usuarioService.AlterarStatusAsync(id, ativo: true, ObterUsuarioLogadoId());
            TempData["MensagemSucesso"] = "Usuário reativado com sucesso.";
        }
        catch (DominioException ex)
        {
            TempData["MensagemErro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { pagina });
    }

    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario is null)
            return NotFound();

        return View(usuario);
    }

    private async Task CarregarGestoresDisponiveis(int idParaExcluir)
    {
        var gestores = await _usuarioService.ListarGestoresAsync(idParaExcluir);
        ViewBag.Gestores = gestores.Select(g => new { g.Id, g.NomeCompleto }).ToList();
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

    private int ObterUsuarioLogadoId()
        => int.Parse(User.FindFirst(ClaimsPersonalizados.UsuarioId)!.Value);
}
