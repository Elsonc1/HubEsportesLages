namespace HubEsportesLages.Domain.Entities;

/// <summary>Mensagem publicada no mural da torcida de um evento ao vivo.</summary>
public class MensagemTorcida
{
    public int Id { get; set; }

    public int EventoId { get; set; }
    public Evento? Evento { get; set; }

    /// <summary>Identificador anônimo do torcedor (cabeçalho X-Torcedor-Id).</summary>
    public string TorcedorId { get; set; } = string.Empty;

    /// <summary>Apelido exibido no mural.</summary>
    public string Autor { get; set; } = "Torcedor";

    public string Texto { get; set; } = string.Empty;

    /// <summary>Marcada como removida pela moderação (deixa de aparecer no mural).</summary>
    public bool Removida { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
