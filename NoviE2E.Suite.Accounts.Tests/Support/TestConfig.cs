using Microsoft.Extensions.Configuration;

namespace NoviE2E.Suite.Accounts.Tests.Support;

public sealed class TestConfig
{
    public required string BaseUrl { get; init; }
    public required Credentials Credentials { get; init; }
    public required PlaywrightOptions Playwright { get; init; }

    public static TestConfig Load()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return config.Get<TestConfig>()
            ?? throw new InvalidOperationException("appsettings.json missing or invalid");
    }
}

public sealed class Credentials
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed class PlaywrightOptions
{
    public bool Headless { get; init; }
    public int SlowMoMs { get; init; }
    public bool TraceOnFailure { get; init; }
    public int DefaultTimeoutMs { get; init; } = 15000;
}
