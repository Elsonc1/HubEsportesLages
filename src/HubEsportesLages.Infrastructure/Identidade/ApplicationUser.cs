using Microsoft.AspNetCore.Identity;

namespace HubEsportesLages.Infrastructure.Identidade;

/// <summary>
/// Usuário da aplicação persistido pelo ASP.NET Core Identity (tabela AspNetUsers).
/// Estende o <see cref="IdentityUser"/> padrão com os campos extras do Hub.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Nome completo do torcedor/organizador (opcional). Usado em saudações e e-mails.</summary>
    public string? NomeCompleto { get; set; }
}
