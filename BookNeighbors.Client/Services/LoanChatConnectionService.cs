
using BookNeighbors.Client.Handlers;
using BookNeighbors.DTO.LoanMessages;
using Microsoft.AspNetCore.SignalR.Client;

namespace BookNeighbors.Client.Services;

public class LoanChatConnectionService(IHttpClientFactory factory, IConfiguration configuration) : IAsyncDisposable
{
    private HubConnection? connection;

    public event Func<LoanMessageResponse, Task>? MessageReceived;
    public event Func<Task>? Reconnected;

    public bool IsConnected => connection?.State == HubConnectionState.Connected;


   /// <summary>
   ///  Stabilisce una connessione al SignalR Hub per la chat dei prestiti e si unisce al gruppo della richiesta di prestito specificata.
   ///  Se esiste già una connessione, la chiude prima di stabilire una nuova connessione.
   ///  Lancia un'eccezione se l'URL base dell'API non è configurato o se il percorso dell'hub SignalR non è configurato.
   /// </summary>
   /// <param name="loanId"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
    public async Task ConnectAsync(Guid loanId)
    {
        if (connection is not null) await connection.DisposeAsync();

        var apiBaseUri = factory.CreateClient("BookNeighborsApi").BaseAddress ?? throw new InvalidOperationException("ApiBaseUrl non configurato.");
        var hubPath = configuration["AppOptions:Services:SignalRHubPath"] ?? configuration["Services:SignalRHubPath"] ?? "hubs/loan-chat";
        var hubUri = new Uri(apiBaseUri, hubPath);

        connection = new HubConnectionBuilder().WithUrl(hubUri, options =>
        {
            options.HttpMessageHandlerFactory = handler => new SignalRCookieHandler { InnerHandler = handler };
        }).WithAutomaticReconnect().Build();

        var currentConnection = connection;

        currentConnection.On<LoanMessageResponse>("ReceiveLoanMessage", async message =>
        {
            if (MessageReceived is not null) await MessageReceived.Invoke(message);
        });

        currentConnection.Reconnected += async _ =>
        {
            await currentConnection.InvokeAsync("JoinLoan", loanId);
            if (Reconnected is not null) await Reconnected.Invoke();
        };

        await currentConnection.StartAsync();
        await currentConnection.InvokeAsync("JoinLoan", loanId);
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null) await connection.DisposeAsync();
    }
}
