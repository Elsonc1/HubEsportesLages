using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>
/// Equipes favoritas do torcedor (acompanhar todos os jogos da equipe).
/// Exige o cabeçalho <c>X-Torcedor-Id</c>.
/// </summary>
[ApiController]
[Route("api/favoritos/equipes")]
[Produces("application/json")]
[Tags("Favoritos")]
public class FavoritosApiController(ITorcidaService torcida) : ControllerBase
{
    /// <summary>Favorita uma equipe para o torcedor atual.</summary>
    [HttpPost("{equipeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Favoritar(int equipeId, CancellationToken ct)
        => Mapear(await torcida.FavoritarEquipeAsync(equipeId, ct));

    /// <summary>Remove o favorito de uma equipe para o torcedor atual.</summary>
    [HttpDelete("{equipeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desfavoritar(int equipeId, CancellationToken ct)
        => Mapear(await torcida.DesfavoritarEquipeAsync(equipeId, ct));

    private IActionResult Mapear(StatusInteracao status) => status switch
    {
        StatusInteracao.Ok => NoContent(),
        StatusInteracao.SemTorcedor => BadRequest(new { mensagem = "Informe o cabeçalho X-Torcedor-Id." }),
        StatusInteracao.NaoEncontrado => NotFound(),
        _ => UnprocessableEntity()
    };
}
