using System.Security.Claims;
using LeveInvestimentos.Application.DTOs;
using LeveInvestimentos.Application.Interfaces;
using LeveInvestimentos.Web.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

/// <summary>Responsável pela autenticação (login/logout) por e-mail e senha.</summary>
public class AccountController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public AccountController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDTO());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO dto, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(dto);

        var usuario = await _usuarioService.AutenticarAsync(dto);
        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(dto);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimsPersonalizados.UsuarioId, usuario.Id.ToString()),
            new(ClaimsPersonalizados.Perfil, usuario.Perfil.ToString()),
            new(ClaimsPersonalizados.NomeCompleto, usuario.NomeCompleto)
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessoNegado() => View();
}
