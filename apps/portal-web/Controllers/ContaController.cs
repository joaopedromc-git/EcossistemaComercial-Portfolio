using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalVendas.Models;
using PortalVendas.Services;

namespace PortalVendas.Controllers;

public sealed class ContaController : Controller
{
    private readonly AutenticacaoService _autenticacao;

    public ContaController(AutenticacaoService autenticacao)
    {
        _autenticacao = autenticacao;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;

        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _autenticacao.AutenticarAsync(
            model.CodigoLogin,
            model.Senha
        );

        if (usuario is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Código ou senha inválidos."
            );

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id
            ),

            new Claim(
                ClaimTypes.Name,
                usuario.NomeVendedor
            ),

            new Claim(
                ClaimTypes.Role,
                usuario.TipoAcesso
            ),

            new Claim(
                "codigo_login",
                usuario.CodigoLogin
            ),

            new Claim(
                "tipo_acesso",
                usuario.TipoAcesso
            ),

            new Claim(
                "primeiro_acesso",
                usuario.PrimeiroAcesso ? "true" : "false"
            )
        };

        if (!string.IsNullOrWhiteSpace(usuario.IdLeads))
        {
            claims.Add(
                new Claim(
                    "id_leads",
                    usuario.IdLeads
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(usuario.Equipe))
        {
            claims.Add(
                new Claim(
                    "equipe",
                    usuario.Equipe
                )
            );
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            }
        );

        if (usuario.PrimeiroAcesso)
        {
            return RedirectToAction(nameof(PrimeiroAcesso));
        }

        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        if (usuario.TipoAcesso == "SUPERIOR")
        {
            return RedirectToAction(
                "EscolherDashboard",
                "Home"
            );
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult PrimeiroAcesso()
    {
        return View(new PrimeiroAcessoViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrimeiroAcesso(
        PrimeiroAcessoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var idUsuario = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(idUsuario))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(Login));
        }

        var sucesso =
            await _autenticacao.AlterarSenhaPrimeiroAcessoAsync(
                idUsuario,
                model.NovaSenha
            );

        if (!sucesso)
        {
            ModelState.AddModelError(
                string.Empty,
                "Não foi possível alterar a senha."
            );

            return View(model);
        }

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        TempData["Mensagem"] =
            "Senha criada com sucesso. Entre novamente usando sua nova senha.";

        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }
}