namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Voto de um torcedor no Jogador da Partida (MVP) de um evento.
/// Restrição de domínio: no máximo um voto por torcedor por evento (índice único).
/// </summary>
public class VotoMvp
{
    public int Id { get; set; }

    public int EventoId { get; set; }
    public Evento? Evento { get; set; }

    public int JogadorEventoId { get; set; }
    public JogadorEvento? Jogador { get; set; }

    /// <summary>Identificador anônimo do torcedor (cabeçalho X-Torcedor-Id).</summary>
    public string TorcedorId { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
