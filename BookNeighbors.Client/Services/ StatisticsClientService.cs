using System.Net.Http.Json;
using BookNeighbors.DTO.Statistics;

namespace BookNeighbors.Client.Services;

public sealed class StatisticsClientService(
    IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient httpClient =
        httpClientFactory.CreateClient("BookNeighborsApi");

    public async Task<DashboardStatisticsResponse> GetOverviewAsync()
    {
        return await httpClient
            .GetFromJsonAsync<DashboardStatisticsResponse>(
                "api/statistics/overview")
            ?? throw new InvalidOperationException(
                "Statistiche non disponibili.");
    }

    public async Task<List<ChartDataResponse>> GetLoanStatusesAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<ChartDataResponse>>(
                "api/statistics/loans") ?? [];
    }

    public async Task<List<ChartDataResponse>> GetLoanTrendAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<ChartDataResponse>>(
                "api/statistics/loans/trend") ?? [];
    }

    public async Task<List<ChartDataResponse>> GetTopBooksAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<ChartDataResponse>>(
                "api/statistics/books/top") ?? [];
    }
}