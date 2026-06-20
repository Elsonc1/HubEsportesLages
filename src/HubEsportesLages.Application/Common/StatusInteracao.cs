namespace HubEsportesLages.Application.Common;

/// <summary>
/// Resultado semântico de uma operação de interação da torcida.
/// O Web traduz cada valor para o status HTTP correspondente.
/// </summary>
public enum StatusInteracao
{
    /// <summary>Operação concluída (ou idempotente: já estava no estado pedido). → 200/204.</summary>
    Ok,
    /// <summary>Evento ou equipe não encontrados. → 404.</summary>
    NaoEncontrado,
    /// <summary>Escrita recusada porque o evento não está AoVivo. → 409.</summary>
    NaoAoVivo,
    /// <summary>Torcedor não identificado (cabeçalho X-Torcedor-Id ausente). → 400.</summary>
    SemTorcedor,
    /// <summary>Dados inválidos (ex.: jogador/opção que não pertence ao evento). → 422.</summary>
    Invalido,
    /// <summary>Limite de envio (rate limit) excedido. → 429.</summary>
    LimiteExcedido
}
