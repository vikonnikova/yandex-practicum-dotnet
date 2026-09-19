namespace Shared.Settings;

public sealed class CacheSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;

    public int EventTtlSeconds { get; set; } = 300;

    public int TopEventsTtlSeconds { get; set; } = 60;
}
