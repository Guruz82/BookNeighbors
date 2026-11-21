using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookNeighbors.Client;
using BookNeighbors.Client.Identity;
using BookNeighbors.Client.Providers;
using BookNeighbors.Client.Interfaces;
using BookNeighbors.Client.Services;
using BookNeighbors.Client.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configurazione della lettura della configurazione dell'applicazione
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));

var apiBaseUrl = builder.Configuration["AppOptions:Services:ApiBaseUrl"];
Console.WriteLine($"ApiBaseUrl: '{apiBaseUrl ?? "NULL"}'");
Console.WriteLine($"Environment: {builder.HostEnvironment.Environment}");
var apiUri = new Uri(apiBaseUrl ?? builder.HostEnvironment.BaseAddress);



// Aggiunta dei componenti radice dell'applicazione
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Aggiunta dei servizi per l'applicazione
builder.Services.AddScoped<LibraryClientService>();
builder.Services.AddScoped<SearchClientService>();
builder.Services.AddScoped<BookClientService>();
builder.Services.AddScoped<ILoanClientService, LoanClientService>();
builder.Services.AddScoped<StatisticsClientService>();
builder.Services.AddScoped<BrowserLocationService>();
builder.Services.AddScoped<NotificationClientService>();
builder.Services.AddTransient<CookieHandler>();
builder.Services.AddTransient<LoanChatConnectionService>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddScoped<IAccountManagement>(sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<LoanMessagesClientService>();

builder.Services.AddTransient<NotificationRealtimeService>();
builder.Services.AddScoped<PersonalLoanStatisticsClientService>();
// Aggiunta dei servizi per l'autenticazione e l'autorizzazione
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();






// Configurazione dei client HTTP per l'applicazione
builder.Services.AddHttpClient("BookNeighborsAntiforgery", client =>
{
    client.BaseAddress = apiUri;
});

builder.Services.AddHttpClient("BookNeighborsApi", client =>
{
    client.BaseAddress = apiUri;
}).AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
