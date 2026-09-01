using System.Diagnostics;
using LeveInvestimentos.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeveInvestimentos.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Erro()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            Mensagem = "Ocorreu um erro inesperado. Tente novamente em instantes."
        });
    }
}
