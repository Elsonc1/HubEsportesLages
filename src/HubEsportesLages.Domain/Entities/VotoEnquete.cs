namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Voto de um torcedor em uma opção de enquete.
/// Restrição de domínio: no máximo um voto por torcedor por enquete (índice único).
/// </summary>
public class VotoEnquete
{
    public int Id { get; set; }

    public int EnqueteId { get; set; }
    public Enquete? Enquete { get; set; }

    public int OpcaoEnqueteId { get; set; }
    public OpcaoEnquete? Opcao { get; set; }

    /// <summary>Identificador anônimo do torcedor (cabeçalho X-Torcedor-Id).</summary>
    public string TorcedorId { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
