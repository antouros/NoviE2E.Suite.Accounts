using Microsoft.Playwright;

namespace NoviE2E.Suite.Accounts.Tests.Support;

/// <summary>
/// Scenario-scoped holder for the Playwright page. Injected into page objects and step definitions via Reqnroll DI.
/// </summary>
public sealed class PlaywrightContext
{
    public IPlaywright Playwright { get; set; } = null!;
    public IBrowser Browser { get; set; } = null!;
    public IBrowserContext BrowserContext { get; set; } = null!;
    public IPage Page { get; set; } = null!;
}
