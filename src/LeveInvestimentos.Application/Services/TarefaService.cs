using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Interfaces;

namespace LeveInvestimentos.Application.Services;

/// <summary>
/// Regras de negócio do agendamento de tarefas: somente gestores atribuem tarefas
/// a seus próprios subordinados, e somente o responsável pode concluí-la.
/// </summary>
public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _tarefaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;

    public TarefaService(
        ITarefaRepository tarefaRepository,
        IUsuarioRepository usuarioRepository,
        IEmailService emailService)
    {
        _tarefaRepository = tarefaRepository;
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
    }

    public async Task<Tarefa> CadastrarAsync(TarefaCadastroDTO dto, int gestorId)
    {
        var gestor = await _usuarioRepository.ObterPorIdAsync(gestorId)
            ?? throw new DominioException("Usuário logado não encontrado.");

        if (!gestor.IsGestor)
            throw new DominioException("Somente usuários com perfil de gestor podem cadastrar tarefas.");

        var responsavel = await _usuarioRepository.ObterPorIdAsync(dto.UsuarioResponsavelId)
            ?? throw new DominioException("Subordinado não encontrado.");

        if (responsavel.GestorId != gestorId)
            throw new DominioException("Você só pode atribuir tarefas aos seus próprios subordinados.");

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

    public async Task<IEnumerable<TarefaListaDTO>> ListarPorGestorAsync(int gestorId)
    {
        var tarefas = await _tarefaRepository.ObterPorGestorAsync(gestorId);
        return MapearParaLista(tarefas);
    }

    public async Task<IEnumerable<TarefaListaDTO>> ListarPorResponsavelAsync(int usuarioId)
    {
        var tarefas = await _tarefaRepository.ObterPorResponsavelAsync(usuarioId);
        return MapearParaLista(tarefas);
    }

    public async Task ConcluirAsync(int tarefaId, int usuarioLogadoId)
    {
        var tarefa = await _tarefaRepository.ObterComUsuariosAsync(tarefaId)
            ?? throw new DominioException("Tarefa não encontrada.");

        if (tarefa.UsuarioResponsavelId != usuarioLogadoId)
            throw new DominioException("Somente o responsável pela tarefa pode concluí-la.");

        if (tarefa.Status == Domain.Enums.StatusTarefa.Concluida)
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
        catch
        {
            // Adicionar log de falha de envio de e-mail para debugging futuro
        }
    }
}
