namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Jogador da escalação de um evento — candidato à votação de "Jogador da Partida" (MVP).
/// Cadastrado pelo organizador para cada jogo.
/// </summary>
public class JogadorEvento
{
    public int Id { get; set; }

    public int EventoId { get; set; }
    public Evento? Evento { get; set; }

    /// <summary>Equipe do jogador (opcional — eventos sem confronto podem não ter).</summary>
    public int? EquipeId { get; set; }
    public Equipe? Equipe { get; set; }

    public string Nome { get; set; } = string.Empty;
}
