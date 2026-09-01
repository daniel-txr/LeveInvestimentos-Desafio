using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Exceptions;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Enums;
using LeveInvestimentos.Domain.Interfaces;

namespace LeveInvestimentos.Application.Services;

/// <summary>
/// Concentra as regras de negócio relacionadas a usuários: autenticação,
/// cadastro (restrito a gestores) e consultas. Não conhece detalhes de
/// infraestrutura (EF Core, SMTP, etc.) — depende apenas de abstrações.
/// </summary>
public class UsuarioService : IUsuarioService
{
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
        var gestorLogado = await _usuarioRepository.ObterPorIdAsync(gestorLogadoId)
            ?? throw new DominioException("Usuário logado não encontrado.");

        if (!gestorLogado.IsGestor)
            throw new DominioException("Somente usuários com perfil de gestor podem cadastrar novos usuários.");

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

    public async Task<IEnumerable<UsuarioListaDTO>> ListarAsync()
    {
        var usuarios = await _usuarioRepository.ObterTodosComGestorAsync();

        return usuarios
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new UsuarioListaDTO
            {
                Id = u.Id,
                NomeCompleto = u.NomeCompleto,
                Email = u.Email,
                TelefoneCelular = u.TelefoneCelular,
                CaminhoFoto = u.CaminhoFoto,
                Perfil = u.Perfil,
                Ativo = u.Ativo,
                NomeGestor = u.Gestor?.NomeCompleto
            });
    }

    public async Task<IEnumerable<Usuario>> ListarSubordinadosAsync(int gestorId)
        => await _usuarioRepository.ObterSubordinadosAsync(gestorId);

    public async Task<Usuario?> ObterPorIdAsync(int id)
        => await _usuarioRepository.ObterPorIdAsync(id);
}
