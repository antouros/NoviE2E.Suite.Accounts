using Microsoft.Playwright;
using NoviE2E.Suite.Accounts.Tests.PageObjects;
using NoviE2E.Suite.Accounts.Tests.Support;
using Reqnroll;
using static Microsoft.Playwright.Assertions;

namespace NoviE2E.Suite.Accounts.Tests.StepDefinitions;

[Binding]
public sealed class LoginSteps
{
    private readonly LoginPage _loginPage;
    private readonly IPage _page;
    private readonly TestConfig _config;

    public LoginSteps(LoginPage loginPage, IPage page)
    {
        _loginPage = loginPage;
        _page = page;
        _config = TestConfig.Load();
    }

    [Given("the user navigates to the site")]
    public async Task TheUserNavigatesToTheSite()
    {
        await _loginPage.NavigateAsync();
    }

    [When("the user logs in with the configured credentials")]
    public async Task TheUserLogsInWithTheConfiguredCredentials()
    {
        await _loginPage.LoginAsync(_config.Credentials.Username, _config.Credentials.Password);
    }

    [Then("the login API confirms success")]
    public void TheLoginApiConfirmsSuccess()
    {
        _loginPage.AssertLoginApiSucceeded();
    }

    [Then("the user is authenticated")]
    public async Task TheUserIsAuthenticated()
    {
        await _loginPage.AssertAuthenticatedAsync();
    }

    [Then(@"the page title is ""(.*)""")]
    public async Task ThePageTitleIs(string expectedTitle)
    {
        // Intentionally wrong assertion — triggers a failure so Playwright captures a trace.
        // Download the trace ZIP from CI artifacts and drop it at https://trace.playwright.dev
        // to time-travel through every step: screenshots, DOM, network, console.
        await Expect(_page).ToHaveTitleAsync(expectedTitle);
    }

    [Then("the user logs out")]
    public async Task TheUserLogsOut()
    {
        await _loginPage.LogoutAsync();
    }

    [Then("the user is signed out")]
    public async Task TheUserIsSignedOut()
    {
        await _loginPage.AssertSignedOutAsync();
    }
}
