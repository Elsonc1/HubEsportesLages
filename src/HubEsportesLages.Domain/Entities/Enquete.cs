namespace HubEsportesLages.Domain.Entities;

/// <summary>Enquete rápida associada a um evento (ex.: "Qual será o resultado final?").</summary>
public class Enquete
{
    public int Id { get; set; }

    public int EventoId { get; set; }
    public Evento? Evento { get; set; }

    public string Pergunta { get; set; } = string.Empty;

    /// <summary>Apenas a enquete ativa de um evento alimenta a tela da torcida.</summary>
    public bool Ativa { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public ICollection<OpcaoEnquete> Opcoes { get; set; } = new List<OpcaoEnquete>();
}
