namespace HubEsportesLages.Application.Common;

/// <summary>Encapsula o <see cref="StatusInteracao"/> e os dados de retorno de uma operação de escrita.</summary>
public record ResultadoInteracao<T>(StatusInteracao Status, T? Dados)
{
    public static ResultadoInteracao<T> Sucesso(T dados) => new(StatusInteracao.Ok, dados);

    public static ResultadoInteracao<T> Falha(StatusInteracao status) => new(status, default);
}
