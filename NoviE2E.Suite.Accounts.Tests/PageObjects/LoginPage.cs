using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Playwright;
using NoviE2E.Suite.Accounts.Tests.Support;
using static Microsoft.Playwright.Assertions;

namespace NoviE2E.Suite.Accounts.Tests.PageObjects;

public sealed class LoginPage
{
    private readonly IPage _page;
    private JsonElement? _loginApiResponse;

    public LoginPage(PlaywrightContext context)
    {
        _page = context.Page;
    }

    // Semantic CSS class, not brittle //*[@data-nov='…'] XPaths.
    private ILocator LoginEntryLink => _page.Locator("a.headerMenuAnonymous_login");
    private ILocator RegisterButton => _page.Locator("button.headerMenuAnonymous_register");

    // The login modal uses <cm-input> wrappers around the real inputs and exposes no stable ids.
    // The modal opens above everything else, so "first visible password + first visible text input" is reliable.
    private ILocator UsernameInput => _page.Locator("input[type='text']:visible, input:not([type]):visible").First;
    private ILocator PasswordInput => _page.Locator("input[type='password']:visible").First;
    private ILocator SubmitButton => _page.GetByRole(AriaRole.Button, new() { Name = "ΣΥΝΔΕΣΗ", Exact = false })
        .Or(_page.Locator("button[type='submit']:visible")).First;

    // Authenticated header: the balance box opens the wallet drawer which hosts the logout button.
    private ILocator InfoContainer => _page.Locator("[data-nov='1682414139']");
    private ILocator LogoutButton => _page.Locator(".walletDetailsHeader_logoutButton");

    public Task NavigateAsync() => _page.GotoAsync("/");

    public async Task LoginAsync(string username, string password)
    {
        // The site opens a welcome popup on first load for anonymous users —
        // the ΣΥΝΔΕΣΗ CTA inside it is the login entry point. Use that directly.
        var loginCtaInPopup = _page.GetByRole(AriaRole.Button, new() { Name = "ΣΥΝΔΕΣΗ", Exact = false });
        try
        {
            await loginCtaInPopup.First.ClickAsync(new LocatorClickOptions { Timeout = 8000 });
        }
        catch (TimeoutException)
        {
            // Popup wasn't there; fall back to the header login link.
            await LoginEntryLink.ClickAsync();
        }

        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);

        // Playwright's click-and-wait-for-response pattern: fires the action and captures the
        // matching API response in one atomic call. No race conditions, no manual Task wiring.
        // Replaces an entire RestSharp + cookie-sharing layer in the Selenium equivalent.
        var response = await _page.RunAndWaitForResponseAsync(
            async () => await SubmitButton.ClickAsync(),
            r => r.Url.Contains("/useraccount/login", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase));

        var body = await response.TextAsync();
        _loginApiResponse = JsonDocument.Parse(body).RootElement.Clone();
    }

    public async Task LogoutAsync()
    {
        await DismissOverlaysAsync();
        await InfoContainer.ClickAsync();
        await LogoutButton.ClickAsync();
    }

    public async Task AssertAuthenticatedAsync()
    {
        await Expect(LoginEntryLink).ToBeHiddenAsync(new() { Timeout = 20000 });
        await Expect(RegisterButton).ToBeHiddenAsync();
    }

    public void AssertLoginApiSucceeded()
    {
        _loginApiResponse.Should().NotBeNull("the POST /useraccount/login response should have been captured during LoginAsync");

        var root = _loginApiResponse!.Value;
        root.TryGetProperty("success", out var successProp)
            .Should().BeTrue("the login response must include a 'success' field");
        successProp.GetBoolean().Should().BeTrue("the login API must report success=true for a valid credential login");
    }

    public async Task AssertSignedOutAsync()
    {
        await Expect(LoginEntryLink).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(RegisterButton).ToBeVisibleAsync();
    }

    private async Task DismissOverlaysAsync()
    {
        // Escape closes any focused CDK modal (welcome promo, cookie banner, etc.).
        await _page.Keyboard.PressAsync("Escape");

        // Strip leftover backdrop DOM that would still intercept pointer events.
        await _page.EvaluateAsync(@"() => {
            document.querySelectorAll('.cdk-overlay-backdrop, .cdk-overlay-container').forEach(e => e.remove());
        }");
    }
}
