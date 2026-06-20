namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Resolve a identidade do torcedor da requisição atual. No MVP é anônima por dispositivo
/// (GUID enviado no cabeçalho <c>X-Torcedor-Id</c>). Evolui para autenticação real sem
/// quebrar o contrato (lacuna #3 do design).
/// </summary>
public interface ITorcedorContexto
{
    /// <summary>Identificador do torcedor, ou null quando não informado.</summary>
    string? TorcedorId { get; }
}
