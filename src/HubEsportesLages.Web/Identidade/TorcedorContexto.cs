using HubEsportesLages.Application.Interfaces;

namespace HubEsportesLages.Web.Identidade;

/// <summary>
/// Identidade anônima do torcedor por dispositivo: lê o GUID do cabeçalho <c>X-Torcedor-Id</c>
/// enviado pelo app. É o fallback até existir autenticação real (lacuna #3 do design).
/// </summary>
public class TorcedorContexto(IHttpContextAccessor accessor) : ITorcedorContexto
{
    public const string Header = "X-Torcedor-Id";

    public string? TorcedorId
    {
        get
        {
            var valor = accessor.HttpContext?.Request.Headers[Header].ToString();
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
