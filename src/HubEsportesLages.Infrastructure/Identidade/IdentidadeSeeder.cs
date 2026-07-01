using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HubEsportesLages.Infrastructure.Identidade;

/// <summary>
/// Garante as roles padrão ("Admin" e "Torcedor") e o usuário administrador inicial.
/// Executado uma vez na inicialização, após as migrations serem aplicadas.
/// </summary>
public static class IdentidadeSeeder
{
    /// <summary>E-mail do administrador padrão do Hub (governança).</summary>
    public const string EmailAdmin = "elsouzalopes@gmail.com";

    /// <summary>Senha forte usada como fallback quando "Admin:SenhaInicial" não estiver configurada.</summary>
    public const string SenhaAdminPadrao = "Admin@Lages2026";

    public static readonly string[] Roles = { "Admin", "Torcedor" };

    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger logger)
    {
        // 1) Garante as roles persistidas (AspNetRoles).
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2) Garante o usuário administrador.
        var admin = await userManager.FindByEmailAsync(EmailAdmin);
        if (admin is null)
        {
            var senhaConfig = configuration["Admin:SenhaInicial"];
            var usandoFallback = string.IsNullOrWhiteSpace(senhaConfig);
            var senha = usandoFallback ? SenhaAdminPadrao : senhaConfig!;

            admin = new ApplicationUser
            {
                UserName = EmailAdmin,
                Email = EmailAdmin,
                EmailConfirmed = true,
                NomeCompleto = "Administrador"
            };

            var resultado = await userManager.CreateAsync(admin, senha);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                if (usandoFallback)
                {
                    // A senha de fallback já é pública (está no código-fonte): logar é apenas
                    // conveniência de dev, com alerta para configurar e trocar em produção.
                    logger.LogWarning(
                        "Admin '{Email}' criado com a SENHA PADRÃO DE DEV '{Senha}'. Configure 'Admin:SenhaInicial' (env/user-secrets) e troque a senha em produção.",
                        EmailAdmin, senha);
                }
                else
                {
                    // Senha veio de configuração (possivelmente real): NUNCA registrar o valor no log.
                    logger.LogInformation(
                        "Admin '{Email}' criado com a senha definida em 'Admin:SenhaInicial'.", EmailAdmin);
                }
            }
            else
            {
                logger.LogError(
                    "Falha ao criar o usuário administrador '{Email}': {Erros}",
                    EmailAdmin,
                    string.Join("; ", resultado.Errors.Select(e => e.Description)));
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            // Usuário já existe mas perdeu a role Admin — restaura.
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
