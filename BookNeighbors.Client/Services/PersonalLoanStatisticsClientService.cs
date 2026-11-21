
using System.Net.Http.Json;
using BookNeighbors.DTO.Statistics;

namespace BookNeighbors.Client.Services;

public class PersonalLoanStatisticsClientService(IHttpClientFactory factory)
{
    /// <summary>
    /// Client HTTP per comunicare con l'API di BookNeighbors.
    /// </summary>
    private readonly HttpClient client = factory.CreateClient("BookNeighborsApi");

    /// <summary>
    /// Restituisce le statistiche relative ai prestiti personali dell'utente autenticato.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<PersonalLoanStatisticsResponse> GetAsync()
    {
        return await client.GetFromJsonAsync<PersonalLoanStatisticsResponse>("api/statistics/my-loans") ?? throw new InvalidOperationException("Statistiche personali non disponibili.");
    }
}
