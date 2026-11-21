using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/statistics")]
public sealed class StatisticsController(IStatisticsService statistics) : ControllerBase
{
    /// <summary>
    /// Restituisce una panoramica delle statistiche del sistema, inclusi i dati aggregati sui prestiti, i libri e gli utenti.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("overview")]
    public async Task<ActionResult<DashboardStatisticsResponse>> Overview(CancellationToken cancellationToken)
    {
        return Ok(await statistics.GetOverviewAsync(cancellationToken));
    }
    /// <summary>
    /// Restituisce i dati statistici sui prestiti, inclusi i conteggi dei prestiti per stato e altre metriche rilevanti.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("loans")]
    public async Task<ActionResult<List<ChartDataResponse>>> Loans(
        CancellationToken cancellationToken)
    {
        return Ok(await statistics.GetLoanStatusesAsync(cancellationToken));
    }
    /// <summary>
    /// Restituisce i dati statistici sui prestiti nel tempo, inclusi i trend dei prestiti per periodo specificato.
    /// Restituisce una lista di oggetti ChartDataResponse che rappresentano i dati dei prestiti nel tempo, con etichette e valori corrispondenti.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    [HttpGet("loans/trend")]
    public async Task<ActionResult<List<ChartDataResponse>>> LoanTrend(CancellationToken cancellationToken)
    {
        return Ok(await statistics.GetLoanTrendAsync(cancellationToken));
    }

    /// <summary>
    /// Restituisce i dati statistici sui libri più popolari, inclusi i libri con il maggior numero di prestiti o interazioni.
    /// Restituisce una lista di oggetti ChartDataResponse che rappresentano i libri più popolari, con etichette e valori corrispondenti al numero di prestiti o interazioni.   
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("books/top")]
    public async Task<ActionResult<List<ChartDataResponse>>> TopBooks(CancellationToken cancellationToken)
    {
        return Ok(await statistics.GetTopBooksAsync(cancellationToken));
    }
}