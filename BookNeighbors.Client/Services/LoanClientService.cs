
using System.Net.Http.Json;
using BookNeighbors.Client.Interfaces;
using BookNeighbors.DTO.Loans;

namespace BookNeighbors.Client.Services;

public sealed class LoanClientService(IHttpClientFactory httpClientFactory) :ILoanClientService
{
    private readonly HttpClient httpClient = 
        httpClientFactory.CreateClient("BookNeighborsApi");

    public async Task<List<LoanResponse>> GetSentAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<LoanResponse>>(
                "api/loan/sent") ?? [];
    }

    public async Task<List<LoanResponse>> GetReceivedAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<LoanResponse>>(
                "api/loan/received") ?? [];
    }

    public async Task<LoanResponse> RequestAsync(Guid bookId)
    {
        using var response = await httpClient.PostAsync(
            $"api/loan/{bookId}", null);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<LoanResponse>()
            ?? throw new InvalidOperationException(
                "Risposta API non valida.");
    }

    public async Task<LoanResponse> ChangeStatusAsync(
        Guid loanId,
        string action)
    {
        if (action is not
            ("accept" or "reject" or "cancel" or "return"))
        {
            throw new ArgumentException(
                "Operazione non valida.", nameof(action));
        }

        using var response = await httpClient.PatchAsync(
            $"api/loan/{loanId}/{action}", null);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<LoanResponse>()
            ?? throw new InvalidOperationException(
                "Risposta API non valida.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();

        var message = response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized =>
                "Devi effettuare il login.",

            System.Net.HttpStatusCode.Conflict =>
                "Operazione non consentita: il libro potrebbe "
                + "non essere disponibile o la richiesta già presente.",

            System.Net.HttpStatusCode.Forbidden =>
                "Non sei autorizzato a eseguire questa operazione.",

            System.Net.HttpStatusCode.NotFound =>
                "Libro o richiesta non trovati.",

            _ => "Operazione non riuscita."
        };

        if (!string.IsNullOrWhiteSpace(content) && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            message = content;
        }

        throw new HttpRequestException(
            message, null, response.StatusCode);
    }
}

