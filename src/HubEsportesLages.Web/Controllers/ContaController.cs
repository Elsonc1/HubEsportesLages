using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Infrastructure.Identidade;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Autenticação do torcedor (usuário comum) via ASP.NET Core Identity.
/// Login, registro e logout com hash de senha, política forte e lockout.
/// </summary>
public class ContaController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailService emailService,
    IEventoService eventos,
    ICatalogoService catalogo) : Controller
{
    [HttpGet("conta")]
    [Authorize]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new ContaViewModel
        {
            Modalidades = await catalogo.ListarModalidadesAsync(ct),
            Equipes = await catalogo.ListarEquipesAsync(ct: ct),
            Proximos = await eventos.ListarProximosAsync(8, ct),
            Metricas = await eventos.ObterMetricasDashboardAsync(ct),
            Inscricao = new CriarInscricaoDto
            {
                Nome = User.Identity?.Name ?? string.Empty,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty
            }
        };

        return View(vm);
    }

    [HttpGet("conta/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("conta/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos.");
            return View();
        }

        // Aceita login por e-mail ou por nome de usuário.
        var alvo = username.Trim();
        var usuario = await userManager.FindByEmailAsync(alvo)
                      ?? await userManager.FindByNameAsync(alvo);

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos.");
            return View();
        }

        var resultado = await signInManager.PasswordSignInAsync(
            usuario, password, isPersistent: true, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        if (resultado.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente em alguns minutos.");
        else
            ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos.");

        return View();
    }

    [HttpGet("conta/registrar")]
    public IActionResult Registrar()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        return View();
    }

    [HttpPost("conta/registrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(string nome, string email, string senha, string confirmarSenha, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nome))
            ModelState.AddModelError(string.Empty, "Informe seu nome completo.");

        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError(string.Empty, "Informe um e-mail válido.");

        if (senha != confirmarSenha)
            ModelState.AddModelError(string.Empty, "As senhas não coincidem.");

        if (!ModelState.IsValid)
            return View();

        var usuario = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            NomeCompleto = nome.Trim()
        };

        // A política de senha forte é validada pelo Identity em CreateAsync.
        var resultado = await userManager.CreateAsync(usuario, senha);
        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, TraduzirErro(erro));
            return View();
        }

        // Todo cadastro novo entra como Torcedor.
        await userManager.AddToRoleAsync(usuario, "Torcedor");

        // Envia e-mail de boas-vindas (vai para o log no ambiente de dev).
        var corpoHtml = $"""
            <div style="font-family: 'Segoe UI', Arial, sans-serif; max-width: 520px; margin: 0 auto; background: #0b2545; color: #fff; border-radius: 12px; padding: 28px;">
                <h2 style="margin: 0 0 12px; color: #22c55e;">🎉 Bem-vindo(a), {nome}!</h2>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Sua conta no <strong>Bora pro Jogo</strong> foi criada com sucesso.</p>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Agora você pode acompanhar eventos, receber notificações e torcer pelas equipes de Lages/SC! 🏟️</p>
                <hr style="border: none; border-top: 1px solid rgba(255,255,255,0.15); margin: 20px 0;" />
                <p style="font-size: 0.82rem; color: #64748b;">Bora pro Jogo · Agenda esportiva de Lages/SC</p>
            </div>
            """;

        await emailService.EnviarAsync("🎉 Bem-vindo ao Bora pro Jogo!", corpoHtml, email.Trim(), ct);

        TempData["RegistroOk"] = $"Conta criada com sucesso para {nome}! Faça login para continuar.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("conta/logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Traduz as mensagens de erro do Identity para português.</summary>
    private static string TraduzirErro(IdentityError erro) => erro.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "Já existe uma conta com este e-mail.",
        "PasswordTooShort" => "A senha deve ter pelo menos 8 caracteres.",
        "PasswordRequiresDigit" => "A senha deve conter pelo menos um número.",
        "PasswordRequiresUpper" => "A senha deve conter pelo menos uma letra maiúscula.",
        "PasswordRequiresLower" => "A senha deve conter pelo menos uma letra minúscula.",
        "PasswordRequiresNonAlphanumeric" => "A senha deve conter pelo menos um caractere especial.",
        "InvalidEmail" => "Informe um e-mail válido.",
        _ => erro.Description
    };
}
