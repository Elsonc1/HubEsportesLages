using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Operações da organização sobre a interação da torcida: definição da escalação
/// (candidatos a MVP), cadastro de enquete e moderação do mural.
/// </summary>
public interface IModeracaoService
{
    /// <summary>Cria e ativa uma enquete para o evento, desativando a anterior.</summary>
    Task<StatusInteracao> CriarEnqueteAsync(int eventoId, CriarEnqueteDto dto, CancellationToken ct = default);

    /// <summary>Define a escalação (candidatos a MVP) do evento, substituindo a existente.</summary>
    Task<StatusInteracao> DefinirEscalacaoAsync(int eventoId, DefinirEscalacaoDto dto, CancellationToken ct = default);

    /// <summary>Remove (oculta) uma mensagem do mural.</summary>
    Task<StatusInteracao> RemoverMensagemAsync(int eventoId, int mensagemId, CancellationToken ct = default);
}
