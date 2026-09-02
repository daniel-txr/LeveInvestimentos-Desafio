using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Enums;
using LeveInvestimentos.Domain.Interfaces;

namespace LeveInvestimentos.Application.Services;

/// <summary>
/// Concentra as regras de negócio relacionadas a usuários: autenticação,
/// cadastro/edição/inativação (restritos a gestores) e consultas. Não conhece detalhes
/// de infraestrutura (EF Core, SMTP, etc.) — depende apenas de abstrações.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private const int TamanhoPaginaPadrao = 10;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHashService _hashService;

    public UsuarioService(IUsuarioRepository usuarioRepository, IHashService hashService)
    {
        _usuarioRepository = usuarioRepository;
        _hashService = hashService;
    }

    public async Task<Usuario?> AutenticarAsync(LoginDTO dto)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email.Trim().ToLowerInvariant());

        if (usuario is null || !usuario.Ativo)
            return null;

        return _hashService.VerificarSenha(dto.Senha, usuario.SenhaHash) ? usuario : null;
    }

    public async Task<Usuario> CadastrarAsync(UsuarioCadastroDTO dto, int gestorLogadoId)
    {
        var gestorLogado = await ObterGestorLogadoOuFalharAsync(gestorLogadoId);

        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        if (await _usuarioRepository.ExisteEmailAsync(emailNormalizado))
            throw new DominioException("Já existe um usuário cadastrado com este e-mail.");

        var usuario = new Usuario
        {
            NomeCompleto = dto.NomeCompleto.Trim(),
            DataNascimento = dto.DataNascimento,
            TelefoneFixo = dto.TelefoneFixo?.Trim(),
            TelefoneCelular = dto.TelefoneCelular.Trim(),
            Email = emailNormalizado,
            Endereco = dto.Endereco.Trim(),
            CaminhoFoto = dto.CaminhoFoto,
            Perfil = dto.Perfil,
            SenhaHash = _hashService.GerarHash(dto.Senha),
            Ativo = true,
            // Se é um subordinado, ele fica ligado ao gestor que o cadastrou.
            // Se é um gestor, null.
            GestorId = dto.Perfil == PerfilUsuario.Subordinado ? gestorLogadoId : null
        };

        await _usuarioRepository.AdicionarAsync(usuario);
        await _usuarioRepository.SalvarAlteracoesAsync();

        return usuario;
    }

    public async Task<Usuario> EditarAsync(UsuarioEdicaoDTO dto, int gestorLogadoId)
    {
        await ObterGestorLogadoOuFalharAsync(gestorLogadoId);

        var usuario = await _usuarioRepository.ObterPorIdAsync(dto.Id)
            ?? throw new DominioException("Usuário não encontrado.");

        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        if (await _usuarioRepository.ExisteEmailAsync(emailNormalizado, idParaIgnorar: usuario.Id))
            throw new DominioException("Já existe outro usuário cadastrado com este e-mail.");

        // Impede que o gestor logado se autodesqualifique sem querer (perderia a permissão
        // de gerenciar o próprio time no meio da sessão, gerando um estado confuso).
        if (usuario.Id == gestorLogadoId && dto.Perfil == PerfilUsuario.Subordinado)
            throw new DominioException("Você não pode alterar o seu próprio perfil para Subordinado.");

        if (dto.Perfil == PerfilUsuario.Subordinado)
        {
            if (dto.GestorId is null)
                throw new DominioException("Selecione o gestor responsável pelo subordinado.");

            if (dto.GestorId == usuario.Id)
                throw new DominioException("Um usuário não pode ser gestor de si mesmo.");

            var novoGestor = await _usuarioRepository.ObterPorIdAsync(dto.GestorId.Value);
            if (novoGestor is null || novoGestor.Perfil != PerfilUsuario.Gestor || !novoGestor.Ativo)
                throw new DominioException("O gestor selecionado é inválido ou está inativo.");

            usuario.GestorId = dto.GestorId;
        }
        else
        {
            // Usuário passou a ser (ou continua sendo) Gestor: gestor não tem gestor acima dele.
            usuario.GestorId = null;
        }

        usuario.NomeCompleto = dto.NomeCompleto.Trim();
        usuario.DataNascimento = dto.DataNascimento;
        usuario.TelefoneFixo = dto.TelefoneFixo?.Trim();
        usuario.TelefoneCelular = dto.TelefoneCelular.Trim();
        usuario.Email = emailNormalizado;
        usuario.Endereco = dto.Endereco.Trim();
        usuario.Perfil = dto.Perfil;

        if (!string.IsNullOrWhiteSpace(dto.CaminhoFoto))
            usuario.CaminhoFoto = dto.CaminhoFoto;

        _usuarioRepository.Atualizar(usuario);
        await _usuarioRepository.SalvarAlteracoesAsync();

        return usuario;
    }

    public async Task AlterarStatusAsync(int usuarioId, bool ativo, int gestorLogadoId)
    {
        await ObterGestorLogadoOuFalharAsync(gestorLogadoId);

        if (usuarioId == gestorLogadoId && !ativo)
            throw new DominioException("Você não pode desativar o seu próprio usuário.");

        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId)
            ?? throw new DominioException("Usuário não encontrado.");

        // Não fazemos exclusão física (DELETE) de propósito: o usuário pode ter tarefas e/ou
        // subordinados vinculados (chaves estrangeiras com DeleteBehavior.Restrict), então uma
        // exclusão física quebraria essas referências e o histórico de tarefas seria perdido.
        // "Excluir" aqui significa inativar — o usuário some das listas de seleção (login,
        // atribuição de tarefas, dropdown de gestor) mas o histórico permanece íntegro.
        usuario.Ativo = ativo;
        _usuarioRepository.Atualizar(usuario);
        await _usuarioRepository.SalvarAlteracoesAsync();
    }

    public async Task<IEnumerable<UsuarioListaDTO>> ListarAsync()
    {
        var usuarios = await _usuarioRepository.ObterTodosComGestorAsync();
        return usuarios.Select(MapearParaListaDTO);
    }

    public async Task<PaginacaoResultado<UsuarioListaDTO>> ListarPaginadoAsync(int pagina, int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina < 1 ? TamanhoPaginaPadrao : tamanhoPagina;

        var (usuarios, total) = await _usuarioRepository.ObterPaginadoComGestorAsync(pagina, tamanhoPagina);

        return new PaginacaoResultado<UsuarioListaDTO>
        {
            Itens = usuarios.Select(MapearParaListaDTO),
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<IEnumerable<Usuario>> ListarSubordinadosAsync(int gestorId)
        => await _usuarioRepository.ObterSubordinadosAsync(gestorId);

    public async Task<IEnumerable<Usuario>> ListarGestoresAsync(int? idParaExcluir = null)
    {
        var gestores = await _usuarioRepository.ObterGestoresAsync();
        return idParaExcluir is null ? gestores : gestores.Where(g => g.Id != idParaExcluir);
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
        => await _usuarioRepository.ObterPorIdAsync(id);

    private async Task<Usuario> ObterGestorLogadoOuFalharAsync(int gestorLogadoId)
    {
        var gestorLogado = await _usuarioRepository.ObterPorIdAsync(gestorLogadoId)
            ?? throw new DominioException("Usuário logado não encontrado.");

        if (!gestorLogado.IsGestor)
            throw new DominioException("Somente usuários com perfil de gestor podem realizar esta ação.");

        return gestorLogado;
    }

    private static UsuarioListaDTO MapearParaListaDTO(Usuario u) => new()
    {
        Id = u.Id,
        NomeCompleto = u.NomeCompleto,
        Email = u.Email,
        TelefoneCelular = u.TelefoneCelular,
        CaminhoFoto = u.CaminhoFoto,
        Perfil = u.Perfil,
        Ativo = u.Ativo,
        NomeGestor = u.Gestor?.NomeCompleto
    };
}
