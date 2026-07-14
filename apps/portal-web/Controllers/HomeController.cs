using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalVendas.Services;

namespace PortalVendas.Controllers;

[Authorize]
public sealed class HomeController : Controller
{
    private readonly MetabaseEmbedService _metabase;

    public HomeController(MetabaseEmbedService metabase)
    {
        _metabase = metabase;
    }

    public IActionResult EscolherDashboard()
    {
        if (User.FindFirst("primeiro_acesso")?.Value == "true")
        {
            return RedirectToAction(
                "PrimeiroAcesso",
                "Conta"
            );
        }

        if (!User.IsInRole("SUPERIOR"))
        {
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    public async Task<IActionResult> Index()
    {
        if (User.FindFirst("primeiro_acesso")?.Value == "true")
        {
            return RedirectToAction(
                "PrimeiroAcesso",
                "Conta"
            );
        }

        if (User.IsInRole("SUPERIOR"))
        {
            return RedirectToAction(nameof(EscolherDashboard));
        }

        ViewData["MetabaseUrl"] =
            await _metabase.GerarUrlDashboardAsync(
                User,
                "COMUM"
            );

        return View();
    }

    [Authorize(Roles = "SUPERIOR")]
    public async Task<IActionResult> Superior()
    {
        if (User.FindFirst("primeiro_acesso")?.Value == "true")
        {
            return RedirectToAction(
                "PrimeiroAcesso",
                "Conta"
            );
        }

        ViewData["MetabaseUrl"] =
            await _metabase.GerarUrlDashboardAsync(
                User,
                "SUPERIOR"
            );

        return View();
    }

    [Authorize(Roles = "SUPERIOR")]
    public async Task<IActionResult> Equipe()
    {
        if (User.FindFirst("primeiro_acesso")?.Value == "true")
        {
            return RedirectToAction(
                "PrimeiroAcesso",
                "Conta"
            );
        }

        ViewData["MetabaseUrl"] =
            await _metabase.GerarUrlDashboardAsync(
                User,
                "EQUIPE"
            );

        return View();
    }
}