using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Domain.Enums;
using LeveInvestimentos.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

/// <summary>
/// Agendamento e acompanhamento de tarefas.
/// - Gestores: cadastram, editam e excluem tarefas de seus subordinados, e acompanham o andamento.
/// - Subordinados: visualizam e concluem as tarefas atribuídas a eles.
/// </summary>
[Authorize]
public class TarefasController : Controller
{
    private const int TamanhoPagina = 9;

    private readonly ITarefaService _tarefaService;
    private readonly IUsuarioService _usuarioService;

    public TarefasController(ITarefaService tarefaService, IUsuarioService usuarioService)
    {
        _tarefaService = tarefaService;
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Gestor vê as tarefas que criou (para acompanhamento); subordinado vê as tarefas
    /// atribuídas a ele (para execução).
    /// </summary>
    public async Task<IActionResult> Index(int pagina = 1)
    {
        var usuarioId = ObterUsuarioLogadoId();
        var isGestor = GestorLogado();

        var resultado = isGestor
            ? await _tarefaService.ListarPorGestorPaginadoAsync(usuarioId, pagina, TamanhoPagina)
            : await _tarefaService.ListarPorResponsavelPaginadoAsync(usuarioId, pagina, TamanhoPagina);

        ViewBag.IsGestor = isGestor;
        return View(resultado);
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
    public async Task<IActionResult> Create()
    {
        await RecarregarSubordinados(ObterUsuarioLogadoId());
        return View(new TarefaCadastroDTO { DataLimite = DateTime.Today.AddDays(7) });
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TarefaCadastroDTO dto)
    {
        var gestorId = ObterUsuarioLogadoId();

        if (!ModelState.IsValid)
        {
            await RecarregarSubordinados(gestorId);
            return View(dto);
        }

        try
        {
            await _tarefaService.CadastrarAsync(dto, gestorId);
            TempData["MensagemSucesso"] = "Tarefa cadastrada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await RecarregarSubordinados(gestorId);
            return View(dto);
        }
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
    public async Task<IActionResult> Edit(int id)
    {
        var gestorId = ObterUsuarioLogadoId();
        var tarefa = await _tarefaService.ObterPorIdAsync(id);

        if (tarefa is null)
            return NotFound();

        if (tarefa.UsuarioGestorId != gestorId)
            return Forbid();

        if (tarefa.Status == StatusTarefa.Concluida)
        {
            TempData["MensagemErro"] = "Uma tarefa já concluída não pode ser editada.";
            return RedirectToAction(nameof(Index));
        }

        await RecarregarSubordinados(gestorId);

        return View(new TarefaEdicaoDTO
        {
            Id = tarefa.Id,
            UsuarioResponsavelId = tarefa.UsuarioResponsavelId,
            Mensagem = tarefa.Mensagem,
            DataLimite = tarefa.DataLimite
        });
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TarefaEdicaoDTO dto)
    {
        var gestorId = ObterUsuarioLogadoId();

        if (!ModelState.IsValid)
        {
            await RecarregarSubordinados(gestorId);
            return View(dto);
        }

        try
        {
            await _tarefaService.EditarAsync(dto, gestorId);
            TempData["MensagemSucesso"] = "Tarefa atualizada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await RecarregarSubordinados(gestorId);
            return View(dto);
        }
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.SomenteGestor)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, int pagina = 1)
    {
        try
        {
            await _tarefaService.ExcluirAsync(id, ObterUsuarioLogadoId());
            TempData["MensagemSucesso"] = "Tarefa excluída com sucesso.";
        }
        catch (DominioException ex)
        {
            TempData["MensagemErro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { pagina });
    }

    /// <summary>Permite que o subordinado responsável marque a tarefa como concluída.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Concluir(int id, int pagina = 1)
    {
        var usuarioId = ObterUsuarioLogadoId();

        try
        {
            await _tarefaService.ConcluirAsync(id, usuarioId);
            TempData["MensagemSucesso"] = "Tarefa concluída.";
        }
        catch (DominioException ex)
        {
            TempData["MensagemErro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { pagina });
    }

    private async Task RecarregarSubordinados(int gestorId)
    {
        var subordinados = await _usuarioService.ListarSubordinadosAsync(gestorId);
        ViewBag.Subordinados = subordinados.Select(s => new { s.Id, s.NomeCompleto }).ToList();
    }

    private int ObterUsuarioLogadoId()
        => int.Parse(User.FindFirst(ClaimsPersonalizados.UsuarioId)!.Value);

    private bool GestorLogado()
        => User.FindFirst(ClaimsPersonalizados.Perfil)?.Value == PerfilUsuario.Gestor.ToString();
}
