using BookNeighbors.DTO.Statistics;

namespace BookNeighbors.Api.Interfaces;

public interface IStatisticsService
{
    /// <summary>
    /// Restituisce le statistiche generali del sistema, come il numero totale di libri, utenti e prestiti. 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DashboardStatisticsResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restituisce le statistiche sugli stati dei prestiti.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<ChartDataResponse>> GetLoanStatusesAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Restituisce le statistiche sull'andamento dei prestiti nel tempo, come il numero di prestiti per mese o per anno.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<ChartDataResponse>> GetLoanTrendAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Restituisce le statistiche sui libri più popolari, come il numero di prestiti per libro o per categoria.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<ChartDataResponse>> GetTopBooksAsync(CancellationToken cancellationToken = default);
}