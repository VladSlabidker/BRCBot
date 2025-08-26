namespace Data.SQL.Configs;

public sealed class SqlExpressConfig
{
    public string? DataSource { get; init; }

    public string? InitialCatalog { get; init; }

    public bool IntegratedSecurity { get; init; }

    public bool TrustServerCertificate { get; init; }
    
    public string? Password { get; init; }
    
    public string? UserId { get; init; }
}