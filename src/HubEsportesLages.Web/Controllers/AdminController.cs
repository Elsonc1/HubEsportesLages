using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Infrastructure.Identidade;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Área de gestão (organizadores/Fundação Municipal de Esportes) para publicar
/// novos eventos na agenda. Protegido por ASP.NET Core Identity (role Admin).
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController(
    IEventoService eventos,
    ICatalogoService catalogo,
    IEmailService emailService,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await MontarViewModelAsync(new CriarEventoDto
        {
            Inicio = DateTime.Today.AddDays(1).AddHours(20)
        }, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEventoDto evento, CancellationToken ct)
    {
        if (evento.EquipeCasaId.HasValue && evento.EquipeCasaId == evento.EquipeVisitanteId)
            ModelState.AddModelError(nameof(evento.EquipeVisitanteId), "As equipes mandante e visitante devem ser diferentes.");

        if (!ModelState.IsValid)
        {
            var vm = await MontarViewModelAsync(evento, ct);
            return View(nameof(Index), vm);
        }

        var id = await eventos.CriarAsync(evento, ct);
        TempData["EventoOk"] = $"Evento publicado com sucesso na agenda (#{id}).";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminViewModel> MontarViewModelAsync(CriarEventoDto evento, CancellationToken ct) => new()
    {
        Evento = evento,
        Modalidades = await catalogo.ListarModalidadesAsync(ct),
        Locais = await catalogo.ListarLocaisAsync(ct),
        Equipes = await catalogo.ListarEquipesAsync(ct: ct),
        Proximos = await eventos.ListarProximosAsync(8, ct),
        Metricas = await eventos.ObterMetricasDashboardAsync(ct)
    };

    [HttpGet("admin/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("admin/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos.");
            return View();
        }

        var alvo = username.Trim();
        var usuario = await userManager.FindByEmailAsync(alvo)
                      ?? await userManager.FindByNameAsync(alvo);

        // Só permite o acesso à área do organizador para quem é Admin.
        if (usuario is null || !await userManager.IsInRoleAsync(usuario, "Admin"))
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

    [HttpGet("admin/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("admin/registrar")]
    [AllowAnonymous]
    public IActionResult Registrar()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        return View();
    }

    [HttpPost("admin/registrar")]
    [AllowAnonymous]
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

        // A política de senha forte é validada pelo Identity. Cadastros públicos
        // entram como Torcedor; a elevação para Admin é feita pela governança.
        var resultado = await userManager.CreateAsync(usuario, senha);
        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, erro.Description);
            return View();
        }

        await userManager.AddToRoleAsync(usuario, "Torcedor");

        // Envia e-mail de boas-vindas.
        var corpoHtml = $"""
            <div style="font-family: 'Segoe UI', Arial, sans-serif; max-width: 520px; margin: 0 auto; background: #0b2545; color: #fff; border-radius: 12px; padding: 28px;">
                <h2 style="margin: 0 0 12px; color: #22c55e;">🎉 Bem-vindo(a), {nome}!</h2>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Sua conta no <strong>Bora pro Jogo</strong> foi criada com sucesso.</p>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Para liberar o acesso de organizador, solicite a elevação à equipe da Fundação Municipal de Esportes.</p>
                <hr style="border: none; border-top: 1px solid rgba(255,255,255,0.15); margin: 20px 0;" />
                <p style="font-size: 0.82rem; color: #64748b;">Bora pro Jogo · Agenda esportiva de Lages/SC</p>
            </div>
            """;

        await emailService.EnviarAsync("🎉 Bem-vindo ao Bora pro Jogo!", corpoHtml, email.Trim(), ct);

        TempData["RegistroOk"] = $"Conta criada com sucesso para {nome}! Faça login para continuar.";
        return RedirectToAction(nameof(Login));
    }
}
