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

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
