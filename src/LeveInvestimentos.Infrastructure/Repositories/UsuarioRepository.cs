using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Enums;
using LeveInvestimentos.Domain.Interfaces;
using LeveInvestimentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Repositories;

public class UsuarioRepository : RepositorioBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext contexto) : base(contexto) { }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExisteEmailAsync(string email, int? idParaIgnorar = null)
        => await DbSet.AnyAsync(u => u.Email == email && (idParaIgnorar == null || u.Id != idParaIgnorar));

    public async Task<IEnumerable<Usuario>> ObterSubordinadosAsync(int gestorId)
        => await DbSet.AsNoTracking()
            .Where(u => u.GestorId == gestorId && u.Ativo)
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();

    public async Task<IEnumerable<Usuario>> ObterGestoresAsync()
        => await DbSet.AsNoTracking()
            .Where(u => u.Perfil == PerfilUsuario.Gestor && u.Ativo)
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();

    public async Task<IEnumerable<Usuario>> ObterTodosComGestorAsync()
        => await DbSet.AsNoTracking()
            .Include(u => u.Gestor)
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();

    public async Task<(IEnumerable<Usuario> Itens, int Total)> ObterPaginadoComGestorAsync(int pagina, int tamanhoPagina)
    {
        var consultaBase = DbSet.AsNoTracking();

        // Conta antes de paginar (Skip/Take), sobre a mesma consulta base, sem o Include
        // (que seria desperdício de JOIN só para contar linhas).
        var total = await consultaBase.CountAsync();

        var itens = await consultaBase
            .Include(u => u.Gestor)
            .OrderBy(u => u.NomeCompleto)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }
}
