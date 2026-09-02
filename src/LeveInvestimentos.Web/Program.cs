using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Application.Services;
using LeveInvestimentos.Domain.Enums;
using LeveInvestimentos.Domain.Interfaces;
using LeveInvestimentos.Infrastructure.Data;
using LeveInvestimentos.Infrastructure.Data.Seed;
using LeveInvestimentos.Infrastructure.Repositories;
using LeveInvestimentos.Infrastructure.Services;
using LeveInvestimentos.Web.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

// ---------- Bootstrap logger ----------
// Logger mínimo, usado apenas até a configuração completa (appsettings.json) estar disponível.
// Isso garante que até uma falha durante o próprio startup (ex.: appsettings.json inválido)
// seja registrada em vez de simplesmente derrubar o processo sem explicação.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando a aplicação Leve Investimentos...");

    var builder = WebApplication.CreateBuilder(args);

    // ---------- Logging (Serilog) ----------
    // Lê toda a configuração (níveis mínimos, sinks de Console e File) da seção "Serilog"
    // do appsettings.json — nada fica hardcoded aqui.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

    // ---------- Persistência ----------
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ---------- Configurações tipadas ----------
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    // ---------- Injeção de dependência (Repositórios / Serviços) ----------
    // Registrados como Scoped: uma instância por requisição HTTP, compatível com o
    // ciclo de vida do DbContext do Entity Framework.
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<ITarefaService, TarefaService>();
    builder.Services.AddScoped<IHashService, HashService>();
    builder.Services.AddScoped<IEmailService, EmailService>();

    // ---------- Autenticação por cookie (e-mail + senha) ----------
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessoNegado";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

    // ---------- Autorização baseada em perfil (somente gestores cadastram usuários/tarefas) ----------
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(PoliticasAutorizacao.SomenteGestor, policy =>
            policy.RequireClaim(ClaimsPersonalizados.Perfil, PerfilUsuario.Gestor.ToString()));
    });

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // ---------- Migração automática + seed do usuário gestor padrão ----------
    using (var scope = app.Services.CreateScope())
    {
        var contexto = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        await DbInitializer.ExecutarAsync(contexto, hashService);
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Erro");
        app.UseHsts();
    }

    // Registra automaticamente cada requisição HTTP (método, rota, status, tempo de resposta)
    // no mesmo log estruturado, no nível Information.
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException é lançada em cenários como "dotnet ef migrations add" (o host
    // é construído e abortado de propósito pela ferramenta) — não é uma falha real.
    Log.Fatal(ex, "A aplicação encerrou de forma inesperada durante o startup.");
}
finally
{
    // Garante que todas as mensagens de log pendentes sejam gravadas antes do processo encerrar.
    Log.CloseAndFlush();
}
