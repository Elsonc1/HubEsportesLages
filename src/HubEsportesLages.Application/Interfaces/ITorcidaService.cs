using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Interação da torcida (torcedor) durante um evento ao vivo: votação de MVP, enquete,
/// mural de mensagens e favoritos de equipe. Escritas exigem o evento <c>AoVivo</c>.
/// </summary>
public interface ITorcidaService
{
    /// <summary>Estado agregado da torcida do evento. Leitura liberada em qualquer status. Null se o evento não existe.</summary>
    Task<TorcidaEstadoDto?> ObterEstadoAsync(string slug, CancellationToken ct = default);

    /// <summary>Registra o voto de MVP (idempotente: 1 por torcedor por evento). Exige evento AoVivo.</summary>
    Task<ResultadoInteracao<TorcidaEstadoDto>> VotarMvpAsync(string slug, VotarMvpDto dto, CancellationToken ct = default);

    /// <summary>Registra o voto na enquete (idempotente: 1 por torcedor por enquete). Exige evento AoVivo.</summary>
    Task<ResultadoInteracao<TorcidaEstadoDto>> VotarEnqueteAsync(string slug, int enqueteId, VotarEnqueteDto dto, CancellationToken ct = default);

    /// <summary>Mensagens do mural (mais recentes primeiro). Null se o evento não existe.</summary>
    Task<IReadOnlyList<MensagemDto>?> ListarMensagensAsync(string slug, DateTime? desde, CancellationToken ct = default);

    /// <summary>Publica uma mensagem no mural. Exige evento AoVivo; aplica limite de tamanho e rate limit por torcedor.</summary>
    Task<ResultadoInteracao<MensagemDto>> EnviarMensagemAsync(string slug, EnviarMensagemDto dto, CancellationToken ct = default);

    /// <summary>Favorita uma equipe para o torcedor atual (idempotente).</summary>
    Task<StatusInteracao> FavoritarEquipeAsync(int equipeId, CancellationToken ct = default);

    /// <summary>Remove o favorito de uma equipe para o torcedor atual (idempotente).</summary>
    Task<StatusInteracao> DesfavoritarEquipeAsync(int equipeId, CancellationToken ct = default);
}
