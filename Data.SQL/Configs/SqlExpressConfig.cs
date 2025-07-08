namespace Data.SQL.Configs;

public sealed class SqlExpressConfig
{
    public string DataSource { get; init; } = default!;

    public string InitialCatalog { get; init; } = default!;

    public bool IntegratedSecurity { get; init; } = true;

    public bool TrustServerCertificate { get; init; } = true;
    
    public string? Password { get; init; } = string.Empty;
}