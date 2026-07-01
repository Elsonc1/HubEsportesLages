namespace HubEsportesLages.Web.Identidade;

/// <summary>
/// Configurações do token JWT Bearer da API REST (consumo pelo app mobile Arena Lages),
/// lidas da seção "Jwt" do appsettings. O cookie do ASP.NET Identity segue sendo o esquema
/// padrão do site MVC — o JWT é adicionado como esquema extra apenas para a API.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Tamanho mínimo (em bytes/chars) exigido da chave HMAC-SHA256.</summary>
    public const int TamanhoMinimoChave = 32;

    /// <summary>Emissor do token (claim "iss"). Validado na entrada.</summary>
    public string Issuer { get; set; } = "BoraProJogo";

    /// <summary>Público-alvo do token (claim "aud"). Validado na entrada.</summary>
    public string Audience { get; set; } = "BoraProJogoApp";

    /// <summary>
    /// Chave secreta simétrica (HMAC-SHA256). Vazia no arquivo versionado: em produção
    /// deve vir da variável de ambiente 'Jwt__SecretKey'. Em Development, se vazia, a app
    /// usa um fallback de dev (com aviso no log) — nunca use esse fallback em produção.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Validade do token em minutos (default 120).</summary>
    public int ExpiraMinutos { get; set; } = 120;
}
