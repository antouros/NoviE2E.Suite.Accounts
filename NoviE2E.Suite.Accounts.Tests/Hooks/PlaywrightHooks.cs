using Microsoft.Playwright;
using NoviE2E.Suite.Accounts.Tests.Support;
using Reqnroll;
using Reqnroll.BoDi;

namespace NoviE2E.Suite.Accounts.Tests.Hooks;

[Binding]
public sealed class PlaywrightHooks
{
    private readonly IObjectContainer _container;
    private readonly ScenarioContext _scenarioContext;

    public PlaywrightHooks(IObjectContainer container, ScenarioContext scenarioContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario(Order = 0)]
    public async Task BeforeScenarioAsync()
    {
        var config = TestConfig.Load();

        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = config.Playwright.Headless,
            SlowMo = config.Playwright.SlowMoMs
        });

        var browserContext = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            Locale = "el-GR",
            BaseURL = config.BaseUrl
        });

        if (config.Playwright.TraceOnFailure)
        {
            await browserContext.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        // Continuously strip promo/tutorial CDK overlays that intercept pointer events.
        // Runs in every frame, every time the page loads, before any scenario interaction.
        await browserContext.AddInitScriptAsync(@"
            const stripOverlays = () => {
                document.querySelectorAll('.cdk-overlay-backdrop, .overlay-more-dark-backdrop').forEach(e => {
                    e.style.pointerEvents = 'none';
                    e.style.display = 'none';
                });
            };
            setInterval(stripOverlays, 200);
            if (document.readyState !== 'loading') stripOverlays();
            else document.addEventListener('DOMContentLoaded', stripOverlays);
        ");

        var page = await browserContext.NewPageAsync();
        page.SetDefaultTimeout(config.Playwright.DefaultTimeoutMs);

        var pwContext = new PlaywrightContext
        {
            Playwright = playwright,
            Browser = browser,
            BrowserContext = browserContext,
            Page = page
        };

        _container.RegisterInstanceAs(pwContext);
        _container.RegisterInstanceAs(page);
    }

    [AfterScenario]
    public async Task AfterScenarioAsync()
    {
        var pwContext = _container.Resolve<PlaywrightContext>();
        var config = TestConfig.Load();

        var scenarioFailed = _scenarioContext.TestError is not null;

        if (config.Playwright.TraceOnFailure && scenarioFailed)
        {
            var tracesDir = Path.Combine(AppContext.BaseDirectory, "traces");
            Directory.CreateDirectory(tracesDir);
            var safeName = string.Concat(_scenarioContext.ScenarioInfo.Title
                .Where(c => char.IsLetterOrDigit(c) || c is '_' or '-' or ' '))
                .Replace(' ', '_');
            var tracePath = Path.Combine(tracesDir, $"{safeName}.zip");

            await pwContext.BrowserContext.Tracing.StopAsync(new TracingStopOptions
            {
                Path = tracePath
            });

            // Also dump the current page HTML + a screenshot to help diagnose selector issues.
            try
            {
                var htmlPath = Path.Combine(tracesDir, $"{safeName}.html");
                var html = await pwContext.Page.ContentAsync();
                await File.WriteAllTextAsync(htmlPath, html);

                var pngPath = Path.Combine(tracesDir, $"{safeName}.png");
                await pwContext.Page.ScreenshotAsync(new PageScreenshotOptions { Path = pngPath, FullPage = true });

                NUnit.Framework.TestContext.Progress.WriteLine($"HTML dump: {htmlPath}");
                NUnit.Framework.TestContext.Progress.WriteLine($"Screenshot: {pngPath}");
            }
            catch (PlaywrightException) { /* page already closed; ignore */ }

            NUnit.Framework.TestContext.Progress.WriteLine($"Trace saved: {tracePath}");
            NUnit.Framework.TestContext.Progress.WriteLine("Open it at https://trace.playwright.dev");
        }
        else if (config.Playwright.TraceOnFailure)
        {
            await pwContext.BrowserContext.Tracing.StopAsync();
        }

        await pwContext.Page.CloseAsync();
        await pwContext.BrowserContext.CloseAsync();
        await pwContext.Browser.CloseAsync();
        pwContext.Playwright.Dispose();
    }
}
