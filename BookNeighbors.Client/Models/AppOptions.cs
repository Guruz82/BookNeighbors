namespace BookNeighbors.Client.Models;
/// <summary>
/// Rappresenta le opzioni di configurazione dell'applicazione.
/// </summary>
public class AppOptions
{
    public ServicesOptions Services { get; set; } = new ServicesOptions();
}


public class ServicesOptions
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string SignalRHubPath { get; set; } = string.Empty;
}