using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

/// <summary>Operações da organização: escalação (candidatos a MVP), enquete e moderação do mural.</summary>
public class ModeracaoService(HubDbContext db) : IModeracaoService
{
    public async Task<StatusInteracao> CriarEnqueteAsync(int eventoId, CriarEnqueteDto dto, CancellationToken ct = default)
    {
        if (!await db.Eventos.AnyAsync(e => e.Id == eventoId, ct))
            return StatusInteracao.NaoEncontrado;

        var opcoes = dto.Opcoes
            .Select(o => o.Trim())
            .Where(o => o.Length > 0)
            .ToList();
        if (opcoes.Count < 2)
            return StatusInteracao.Invalido;

        // Garante uma única enquete ativa por evento.
        await db.Enquetes
            .Where(e => e.EventoId == eventoId && e.Ativa)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Ativa, false), ct);

        db.Enquetes.Add(new Enquete
        {
            EventoId = eventoId,
            Pergunta = dto.Pergunta.Trim(),
            Ativa = true,
            CriadoEm = DateTime.Now,
            Opcoes = opcoes.Select(t => new OpcaoEnquete { Texto = t }).ToList()
        });
        await db.SaveChangesAsync(ct);
        return StatusInteracao.Ok;
    }

    public async Task<StatusInteracao> DefinirEscalacaoAsync(int eventoId, DefinirEscalacaoDto dto, CancellationToken ct = default)
    {
        if (!await db.Eventos.AnyAsync(e => e.Id == eventoId, ct))
            return StatusInteracao.NaoEncontrado;

        var jogadores = dto.Jogadores
            .Where(j => !string.IsNullOrWhiteSpace(j.Nome))
            .ToList();
        if (jogadores.Count == 0)
            return StatusInteracao.Invalido;

        // Substitui a escalação: redefinir os candidatos invalida os votos de MVP anteriores.
        await db.VotosMvp.Where(v => v.EventoId == eventoId).ExecuteDeleteAsync(ct);
        await db.JogadoresEvento.Where(j => j.EventoId == eventoId).ExecuteDeleteAsync(ct);

        db.JogadoresEvento.AddRange(jogadores.Select(j => new JogadorEvento
        {
            EventoId = eventoId,
            Nome = j.Nome.Trim(),
            EquipeId = j.EquipeId
        }));
        await db.SaveChangesAsync(ct);
        return StatusInteracao.Ok;
    }

    public async Task<StatusInteracao> RemoverMensagemAsync(int eventoId, int mensagemId, CancellationToken ct = default)
    {
        var mensagem = await db.MensagensTorcida
            .FirstOrDefaultAsync(m => m.Id == mensagemId && m.EventoId == eventoId, ct);
        if (mensagem is null)
            return StatusInteracao.NaoEncontrado;

        mensagem.Removida = true;
        await db.SaveChangesAsync(ct);
        return StatusInteracao.Ok;
    }
}
