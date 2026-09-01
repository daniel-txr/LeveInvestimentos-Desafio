using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Domain.Entities;
using LeveInvestimentos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeveInvestimentos.Infrastructure.Data.Seed;

/// <summary>
/// Garante a existência do usuário gestor padrão inicial, solicitado pelo gestor
/// da área operacional, responsável por cadastrar os primeiros usuários do sistema.
/// É idempotente: pode ser executado várias vezes sem duplicar o registro.
/// </summary>
public static class DbInitializer
{
    private const string EmailUsuarioPadrao = "ti@leveinvestimentos.com.br";
    private const string SenhaUsuarioPadrao = "teste123";

    public static async Task ExecutarAsync(ApplicationDbContext contexto, IHashService hashService)
    {
        await contexto.Database.MigrateAsync();

        var jaExiste = await contexto.Usuarios.AnyAsync(u => u.Email == EmailUsuarioPadrao);
        if (jaExiste)
            return;

        var usuarioPadrao = new Usuario
        {
            NomeCompleto = "TI",
            DataNascimento = new DateTime(1990, 1, 1),
            TelefoneCelular = "(11) 90000-0000",
            Email = EmailUsuarioPadrao,
            Endereco = "Não informado",
            Perfil = PerfilUsuario.Gestor,
            Ativo = true,
            SenhaHash = hashService.GerarHash(SenhaUsuarioPadrao)
        };

        contexto.Usuarios.Add(usuarioPadrao);
        await contexto.SaveChangesAsync();
    }
}
