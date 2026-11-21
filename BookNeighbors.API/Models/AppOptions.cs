namespace BookNeighbors.Api.Models;

public class AppOptions
{
    public ExternalServicesOptions ExternalServices { get; set; } = new ExternalServicesOptions();
    public ClientOptions Client { get; set; } = new ClientOptions();
    public OracleStorageOptions OracleStorage { get; set; } = new();
}
public class ExternalServicesOptions
{
    public string GoogleBooksBaseUrl { get; set; } = string.Empty;
    public string OpenLibraryBaseUrl { get; set; } = string.Empty;
}
public class ClientOptions
{
    public List<string> AllowedOrigins { get; set; } = new List<string>();
}

public class OracleStorageOptions
{
    public string ReadUrl { get; set; } = string.Empty;
    public string WriteUrl { get; set; } = string.Empty;
}