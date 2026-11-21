
using System.Net.Http.Json;
using BookNeighbors.DTO.LoanMessages;

namespace BookNeighbors.Client.Services;

public class LoanMessagesClientService(IHttpClientFactory factory)
{
    private readonly HttpClient client = factory.CreateClient("BookNeighborsApi");

    public async Task<List<LoanMessageResponse>> GetMessagesAsync(Guid loanId)
    {
        return await client.GetFromJsonAsync<List<LoanMessageResponse>>($"api/loans/{loanId}/messages") ?? [];
    }

    public async Task<LoanMessageResponse> SendMessageAsync(Guid loanId, string text)
    {
        using var response = await client.PostAsJsonAsync($"api/loans/{loanId}/messages", new LoanMessageRequest(text));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<LoanMessageResponse>() ?? throw new InvalidOperationException("Risposta API non valida.");
    }
}
