using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Enums;
using LeveInvestimentos.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeveInvestimentos.Application.Services;

/// <summary>
/// Regras de negócio do agendamento de tarefas: somente gestores atribuem, editam e excluem
/// tarefas de seus próprios subordinados, e somente o responsável pode concluí-la.
/// </summary>
public class TarefaService : ITarefaService
{
    private const int TamanhoPaginaPadrao = 10;

    private readonly ITarefaRepository _tarefaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<TarefaService> _logger;

    public TarefaService(
        ITarefaRepository tarefaRepository,
        IUsuarioRepository usuarioRepository,
        IEmailService emailService,
        ILogger<TarefaService> logger)
    {
        _tarefaRepository = tarefaRepository;
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Tarefa> CadastrarAsync(TarefaCadastroDTO dto, int gestorId)
    {
        var gestor = await ObterGestorOuFalharAsync(gestorId);
        var responsavel = await ObterSubordinadoDoGestorOuFalharAsync(dto.UsuarioResponsavelId, gestorId);

        if (dto.DataLimite.Date < DateTime.UtcNow.Date)
            throw new DominioException("A data limite não pode estar no passado.");

        var tarefa = new Tarefa
        {
            Mensagem = dto.Mensagem.Trim(),
            DataLimite = dto.DataLimite,
            UsuarioResponsavelId = responsavel.Id,
            UsuarioGestorId = gestor.Id
        };

        await _tarefaRepository.AdicionarAsync(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        await NotificarEmailAsync(
            responsavel.Email,
            "Nova tarefa atribuída a você",
            $"""
             <p>Olá, {responsavel.NomeCompleto},</p>
             <p>Uma nova tarefa foi atribuída a você por <strong>{gestor.NomeCompleto}</strong>:</p>
             <blockquote>{tarefa.Mensagem}</blockquote>
             <p><strong>Prazo:</strong> {tarefa.DataLimite:dd/MM/yyyy}</p>
             """);

        return tarefa;
    }

    public async Task<Tarefa> EditarAsync(TarefaEdicaoDTO dto, int gestorLogadoId)
    {
        await ObterGestorOuFalharAsync(gestorLogadoId);

        var tarefa = await _tarefaRepository.ObterComUsuariosAsync(dto.Id)
            ?? throw new DominioException("Tarefa não encontrada.");

        if (tarefa.UsuarioGestorId != gestorLogadoId)
            throw new DominioException("Você só pode editar tarefas que você mesmo cadastrou.");

        if (tarefa.Status == StatusTarefa.Concluida)
            throw new DominioException("Uma tarefa já concluída não pode ser editada.");

        var responsavel = await ObterSubordinadoDoGestorOuFalharAsync(dto.UsuarioResponsavelId, gestorLogadoId);

        if (dto.DataLimite.Date < DateTime.UtcNow.Date)
            throw new DominioException("o prazo não pode estar vencido.");

        var responsavelMudou = tarefa.UsuarioResponsavelId != responsavel.Id;

        tarefa.Mensagem = dto.Mensagem.Trim();
        tarefa.DataLimite = dto.DataLimite;
        tarefa.UsuarioResponsavelId = responsavel.Id;

        _tarefaRepository.Atualizar(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        // Se o responsável foi trocado, o novo responsável precisa ser avisado —
        // do ponto de vista dele, é uma tarefa nova chegando.
        if (responsavelMudou)
        {
            await NotificarEmailAsync(
                responsavel.Email,
                "Tarefa atribuída a você",
                $"""
                 <p>Olá, {responsavel.NomeCompleto},</p>
                 <p>A tarefa abaixo foi atribuída a você:</p>
                 <blockquote>{tarefa.Mensagem}</blockquote>
                 <p><strong>Prazo:</strong> {tarefa.DataLimite:dd/MM/yyyy}</p>
                 """);
        }

        return tarefa;
    }

    public async Task ExcluirAsync(int tarefaId, int gestorLogadoId)
    {
        await ObterGestorOuFalharAsync(gestorLogadoId);

        var tarefa = await _tarefaRepository.ObterPorIdAsync(tarefaId)
            ?? throw new DominioException("Tarefa não encontrada.");

        if (tarefa.UsuarioGestorId != gestorLogadoId)
            throw new DominioException("Você só pode excluir tarefas que você mesmo cadastrou.");

        _tarefaRepository.Remover(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();
    }

    public async Task<Tarefa?> ObterPorIdAsync(int tarefaId)
        => await _tarefaRepository.ObterComUsuariosAsync(tarefaId);

    public async Task<IEnumerable<TarefaListaDTO>> ListarPorGestorAsync(int gestorId)
        => MapearParaLista(await _tarefaRepository.ObterPorGestorAsync(gestorId));

    public async Task<IEnumerable<TarefaListaDTO>> ListarPorResponsavelAsync(int usuarioId)
        => MapearParaLista(await _tarefaRepository.ObterPorResponsavelAsync(usuarioId));

    public async Task<PaginacaoResultado<TarefaListaDTO>> ListarPorGestorPaginadoAsync(int gestorId, int pagina, int tamanhoPagina)
    {
        (pagina, tamanhoPagina) = NormalizarPaginacao(pagina, tamanhoPagina);

        var (tarefas, total) = await _tarefaRepository.ObterPorGestorPaginadoAsync(gestorId, pagina, tamanhoPagina);

        return new PaginacaoResultado<TarefaListaDTO>
        {
            Itens = MapearParaLista(tarefas),
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<PaginacaoResultado<TarefaListaDTO>> ListarPorResponsavelPaginadoAsync(int usuarioId, int pagina, int tamanhoPagina)
    {
        (pagina, tamanhoPagina) = NormalizarPaginacao(pagina, tamanhoPagina);

        var (tarefas, total) = await _tarefaRepository.ObterPorResponsavelPaginadoAsync(usuarioId, pagina, tamanhoPagina);

        return new PaginacaoResultado<TarefaListaDTO>
        {
            Itens = MapearParaLista(tarefas),
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task ConcluirAsync(int tarefaId, int usuarioLogadoId)
    {
        var tarefa = await _tarefaRepository.ObterComUsuariosAsync(tarefaId)
            ?? throw new DominioException("Tarefa não encontrada.");

        if (tarefa.UsuarioResponsavelId != usuarioLogadoId)
            throw new DominioException("Somente o responsável pela tarefa pode concluí-la.");

        if (tarefa.Status == StatusTarefa.Concluida)
            throw new DominioException("Esta tarefa já foi concluída.");

        tarefa.Concluir();
        _tarefaRepository.Atualizar(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        if (tarefa.UsuarioGestor is not null)
        {
            await NotificarEmailAsync(
                tarefa.UsuarioGestor.Email,
                "Tarefa finalizada",
                $"""
                 <p>Olá, {tarefa.UsuarioGestor.NomeCompleto},</p>
                 <p>A tarefa abaixo, atribuída a <strong>{tarefa.UsuarioResponsavel?.NomeCompleto}</strong>, foi concluída:</p>
                 <blockquote>{tarefa.Mensagem}</blockquote>
                 <p><strong>Concluída em:</strong> {tarefa.DataConclusao:dd/MM/yyyy HH:mm}</p>
                 """);
        }
    }

    private async Task<Usuario> ObterGestorOuFalharAsync(int gestorId)
    {
        var gestor = await _usuarioRepository.ObterPorIdAsync(gestorId)
            ?? throw new DominioException("Usuário logado não encontrado.");

        if (!gestor.IsGestor)
            throw new DominioException("Somente usuários com perfil de gestor podem realizar esta ação.");

        return gestor;
    }

    private async Task<Usuario> ObterSubordinadoDoGestorOuFalharAsync(int usuarioResponsavelId, int gestorId)
    {
        var responsavel = await _usuarioRepository.ObterPorIdAsync(usuarioResponsavelId)
            ?? throw new DominioException("Subordinado não encontrado.");

        if (responsavel.GestorId != gestorId)
            throw new DominioException("Você só pode atribuir tarefas aos seus próprios subordinados.");

        return responsavel;
    }

    private static (int Pagina, int TamanhoPagina) NormalizarPaginacao(int pagina, int tamanhoPagina)
        => (pagina < 1 ? 1 : pagina, tamanhoPagina < 1 ? TamanhoPaginaPadrao : tamanhoPagina);

    private static IEnumerable<TarefaListaDTO> MapearParaLista(IEnumerable<Tarefa> tarefas)
        => tarefas
            .OrderBy(t => t.DataLimite)
            .Select(t => new TarefaListaDTO
            {
                Id = t.Id,
                Mensagem = t.Mensagem,
                DataLimite = t.DataLimite,
                DataConclusao = t.DataConclusao,
                Status = t.Status,
                Atrasada = t.Atrasada,
                UsuarioResponsavelNome = t.UsuarioResponsavel?.NomeCompleto ?? string.Empty,
                UsuarioGestorNome = t.UsuarioGestor?.NomeCompleto ?? string.Empty
            });

    /// <summary>
    /// Envia a notificação.
    /// </summary>
    private async Task NotificarEmailAsync(string destinatario, string assunto, string corpoHtml)
    {
        try
        {
            await _emailService.EnviarAsync(destinatario, assunto, corpoHtml);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao enviar e-mail de notificação. Destinatário: {Destinatario} | Assunto: {Assunto}",
                destinatario,
                assunto);
        }
    }
}
