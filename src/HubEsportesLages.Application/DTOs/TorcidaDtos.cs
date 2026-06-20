using System.ComponentModel.DataAnnotations;
using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Application.DTOs;

// ─── Estado agregado da tela da torcida ───────────────────────────────────────

/// <summary>Estado completo da tela da torcida de um evento (MVP + enquete + mural + favorito).</summary>
public record TorcidaEstadoDto(
    StatusEvento EventoStatus,
    bool AceitaInteracao,
    MvpDto Mvp,
    EnqueteDto? Enquete,
    IReadOnlyList<MensagemDto> Mensagens,
    IReadOnlyList<int> EquipesFavoritas);

/// <summary>Votação de Jogador da Partida (MVP).</summary>
public record MvpDto(
    IReadOnlyList<MvpCandidatoDto> Candidatos,
    int? MeuVotoJogadorId);

public record MvpCandidatoDto(
    int JogadorEventoId,
    string Nome,
    string? Equipe,
    int Votos);

/// <summary>Enquete rápida com percentuais por opção.</summary>
public record EnqueteDto(
    int Id,
    string Pergunta,
    IReadOnlyList<OpcaoEnqueteDto> Opcoes,
    int? MinhaOpcaoId);

public record OpcaoEnqueteDto(
    int Id,
    string Texto,
    int Votos,
    int Percentual);

/// <summary>Mensagem do mural da torcida.</summary>
public record MensagemDto(
    int Id,
    string Autor,
    string Texto,
    DateTime CriadoEm);

// ─── Comandos do torcedor ─────────────────────────────────────────────────────

/// <summary>Voto no Jogador da Partida.</summary>
public class VotarMvpDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um jogador.")]
    public int JogadorEventoId { get; set; }
}

/// <summary>Voto em uma opção da enquete.</summary>
public class VotarEnqueteDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma opção.")]
    public int OpcaoId { get; set; }
}

/// <summary>Mensagem enviada ao mural.</summary>
public class EnviarMensagemDto
{
    [Required(ErrorMessage = "Escreva uma mensagem.")]
    [StringLength(140, MinimumLength = 1, ErrorMessage = "A mensagem deve ter entre 1 e 140 caracteres.")]
    public string Texto { get; set; } = string.Empty;
}

// ─── Comandos da organização ──────────────────────────────────────────────────

/// <summary>Cadastro de uma enquete (pergunta + opções) para um evento.</summary>
public class CriarEnqueteDto
{
    [Required(ErrorMessage = "Informe a pergunta.")]
    [StringLength(200, MinimumLength = 4)]
    public string Pergunta { get; set; } = string.Empty;

    [MinLength(2, ErrorMessage = "Informe pelo menos duas opções.")]
    public List<string> Opcoes { get; set; } = new();
}

/// <summary>Definição da escalação (candidatos a MVP) de um evento.</summary>
public class DefinirEscalacaoDto
{
    [MinLength(1, ErrorMessage = "Informe pelo menos um jogador.")]
    public List<JogadorEscalacaoDto> Jogadores { get; set; } = new();
}

public class JogadorEscalacaoDto
{
    [Required(ErrorMessage = "Informe o nome do jogador.")]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    public int? EquipeId { get; set; }
}
