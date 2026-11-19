using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookNeighbors.Client;
using BookNeighbors.Client.Identity;
using BookNeighbors.Client.Providers;
using BookNeighbors.Client.Interfaces;
using BookNeighbors.Client.Services;    

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped<LibraryClientService>();
builder.Services.AddScoped<SearchClientService>();
builder.Services.AddScoped<BookClientService>();
builder.Services.AddScoped<ILoanClientService,LoanClientService>();


builder.Services.AddTransient<CookieHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();


builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();


builder.Services.AddScoped<IAccountManagement>(sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());
 
builder.Services.AddHttpClient("BookNeighborsAntiforgery", client =>
{
    client.BaseAddress = new Uri("https://localhost:7225/");
});

builder.Services.AddHttpClient("BookNeighborsApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7225/");
})
.AddHttpMessageHandler<CookieHandler>();


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
