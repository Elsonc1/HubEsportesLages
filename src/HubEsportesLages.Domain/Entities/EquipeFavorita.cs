namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Equipe favoritada por um torcedor (para acompanhar todos os jogos dela).
/// Restrição de domínio: no máximo um registro por torcedor por equipe (índice único).
/// </summary>
public class EquipeFavorita
{
    public int Id { get; set; }

    /// <summary>Identificador anônimo do torcedor (cabeçalho X-Torcedor-Id).</summary>
    public string TorcedorId { get; set; } = string.Empty;

    public int EquipeId { get; set; }
    public Equipe? Equipe { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
