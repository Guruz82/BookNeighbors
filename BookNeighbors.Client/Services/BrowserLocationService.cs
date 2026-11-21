using Microsoft.JSInterop;

namespace BookNeighbors.Client.Services;

public sealed record BrowserPosition(
    double Latitude,
    double Longitude
);

public sealed class BrowserLocationService(
    IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? module;

    public async Task<BrowserPosition> GetCurrentPositionAsync()
    {
        module ??= await js.InvokeAsync<IJSObjectReference>( "import", "./js/browserLocation.js");

        return await module.InvokeAsync<BrowserPosition>( "getCurrentPosition");
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
            await module.DisposeAsync();
    }
}