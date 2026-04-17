using NoviE2E.Suite.Accounts.Tests.PageObjects;
using NoviE2E.Suite.Accounts.Tests.Support;
using Reqnroll;

namespace NoviE2E.Suite.Accounts.Tests.StepDefinitions;

[Binding]
public sealed class LoginSteps
{
    private readonly LoginPage _loginPage;
    private readonly TestConfig _config;

    public LoginSteps(LoginPage loginPage)
    {
        _loginPage = loginPage;
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
