namespace HubEsportesLages.Domain.Entities;

/// <summary>Opção de resposta de uma <see cref="Enquete"/>.</summary>
public class OpcaoEnquete
{
    public int Id { get; set; }

    public int EnqueteId { get; set; }
    public Enquete? Enquete { get; set; }

    public string Texto { get; set; } = string.Empty;
}
