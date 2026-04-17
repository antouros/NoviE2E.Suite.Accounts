# NoviE2E.Suite.Accounts

A minimal Playwright + Reqnroll + NUnit + .NET 8 showcase of the Accounts vertical's test automation, built as a side-by-side comparison against the current QA.Novi Selenium implementation.

**One scenario migrated:** `Verification after account creation`

Source: `QA.Novi.Tests.Account/Features/Verification/VerificationGUI.feature`

---

## Side-by-Side: Same Test, Two Frameworks

### Selenium (current QA.Novi)

```csharp
// Registration page — brittle XPath + data-nov IDs that break on UI refactors
private readonly By _emailField    = By.XPath("//cm-input[@data-nov='1664526453']//input");
private readonly By _passwordField = By.XPath("//cm-input[@data-nov='1664521192']//input[@type='password']");
private readonly By _openAccountBtn = By.XPath("//*[@data-nov='1664522623']");

public void FillEmail(string email)
{
    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    wait.Until(Ec.ElementToBeClickable(_emailField));
    _driver.FindElement(_emailField).SendKeys(email);
}

// OTP step — requires a separate RestSharp-backed helper that hits the real backend
[Then("user successfully completes verification")]
public void UserCompletesTheVerificationStep()
{
    _verification.GetVerificationModel(VerificationReason.Registration);
    var codes = _verification.GetOtpCodes(VerificationReason.Registration.ToString());
    var codeArray = codes.Select(c => c.Code).ToArray();
    EnterOtpCodes(codeArray);

    if (_settings.Channel == Channel.Android)
        _verificationPopUp.IsVerificationCompleted.Should().BeTrue();
    if (_settings.Novi.DomainInfo.Domain != Domain.BR)
        _limitsAndBonusPopUp.IsLimitsBonusPanelEnabled.Should().BeTrue();
}
```

Dependencies the Selenium version pulls in:
- `QA.Novi.Tests.Common` (shared hooks, helpers)
- `QA.Novi.Services` (20+ RestSharp service classes)
- `QA.Novi.Pages.BackOffices`
- `QA.Novi.Pages`
- `QA.Novi.DataAccess`
- `QA.Novi.Utils`
- `QA.Novi.Plugins.Variants`
- 35+ BlueBrown NuGet packages

### Playwright (this repo)

```csharp
// Page object — user-intent locators that survive UI refactors
private ILocator EmailInput       => _page.GetByLabel("Email");
private ILocator PasswordInput    => _page.GetByLabel("Password");
private ILocator OpenAccountBtn   => _page.GetByRole(AriaRole.Button, new() { Name = "Open account" });

public Task FillEmailAsync(string email) => EmailInput.FillAsync(email);
// No WebDriverWait. Auto-wait is built in. That's it.

// OTP step — no backend, no RestSharp, no helper service.
[Then("the user successfully completes verification")]
public async Task TheUserSuccessfullyCompletesVerification()
{
    await _verification.EnterOtpAsync(OtpMock.MockedOtp);
    await _verification.ConfirmAsync();
    await _verification.AssertVerificationCompletedAsync();
}

// The OTP endpoint is intercepted by Playwright's built-in routing:
await page.RouteAsync("**/api/verification/otp/**", route =>
    route.FulfillAsync(new() { Status = 200, ContentType = "application/json",
                               Body = "{\"code\":\"1234\"}" }));
```

Dependencies this repo pulls in:
- `Microsoft.Playwright`
- `Reqnroll.NUnit`
- `NUnit`
- `Bogus` (random user data)
- `AwesomeAssertions`

That's it. ~800 LOC vs 168,000 LOC.

---

## What This Showcases

| Feature | Selenium | Playwright |
|---------|----------|-----------|
| Element waiting | Explicit `WebDriverWait.Until(...)` on every action | Built-in auto-wait: Attached → Visible → Stable → Enabled |
| Locator strategy | XPath + CSS + `data-nov` IDs (break on refactor) | `GetByRole`, `GetByLabel`, `GetByText` (survive refactors) |
| API mocking | Not supported — needs BrowserMob / mitmproxy / custom middleware | Built-in `page.RouteAsync` — mock any HTTP or WebSocket |
| Backend dependency for OTP | Real RestSharp helper hitting live staging | Fully mocked inside the test |
| Debugging a failure | Re-run locally with breakpoints, 30-60 min to diagnose | Auto-captured trace: screenshots + DOM + network + console, diagnose in minutes |
| Test isolation | Shared infra across 9 verticals | Standalone repo, owned by Accounts team |
| Build time | 1 min 25 sec for full QA.Novi solution | Seconds — only this project |

---

## How to Run

```powershell
# From the repo root
cd NoviE2E.Suite.Accounts

# 1. Create your local appsettings.json from the template and fill in credentials
#    (appsettings.json is gitignored so secrets never hit version control).
copy NoviE2E.Suite.Accounts.Tests\appsettings.template.json NoviE2E.Suite.Accounts.Tests\appsettings.json
# then edit appsettings.json and replace REPLACE_WITH_YOUR_USERNAME / _PASSWORD

# 2. Restore NuGet packages
dotnet restore

# 3. Build the project
dotnet build

# 4. Install Playwright browsers (required once per machine)
.\NoviE2E.Suite.Accounts.Tests\bin\Debug\net8.0\playwright.ps1 install chromium

# 5. Run the test
dotnet test
```

---

## Viewing Failures with Trace Viewer

On failure, a trace `.zip` is auto-saved to:

```
NoviE2E.Suite.Accounts.Tests/bin/Debug/net8.0/traces/
```

Open it at **https://trace.playwright.dev** (fully local, nothing leaves your machine) to get:

- Time-travel snapshots of the browser at each step
- Screenshots, DOM, network requests, console logs
- Action timeline with full source context

No equivalent exists in Selenium.

---

## Structure

```
NoviE2E.Suite.Accounts.Tests/
├── appsettings.json              # BaseUrl, headless mode, trace settings
├── reqnroll.json                 # Gherkin language config
├── Features/
│   └── Verification.feature      # ONE scenario
├── StepDefinitions/
│   ├── RegistrationSteps.cs
│   ├── LoginSteps.cs
│   └── VerificationSteps.cs
├── PageObjects/
│   ├── RegistrationPage.cs       # GetByLabel / GetByRole
│   ├── LoginPage.cs
│   └── VerificationPage.cs
├── Hooks/
│   └── PlaywrightHooks.cs        # Browser lifecycle + trace capture
└── Support/
    ├── TestConfig.cs             # Loads appsettings.json
    ├── PlaywrightContext.cs      # Scenario-scoped IPage holder
    ├── TestUser.cs               # Random user data
    ├── OtpMock.cs                # Playwright routing mock
    └── ScenarioKeys.cs           # Constants for ScenarioContext
```

---

## Notes on Scope

This is a **showcase**, not a production-ready replacement. The scenario runs against the real `stg.novibet.gr` site and demonstrates:

- **Playwright launch + navigation** in headed Chromium against staging
- **Locator strategy** — semantic CSS classes instead of XPath with magic `data-nov` IDs
- **Auto-wait** — no `WebDriverWait`, no explicit `ElementToBeClickable`, no `Thread.Sleep`
- **Built-in API mocking** — the `OtpMock` route handler intercepts a real `fetch()` call from inside the browser and returns the mocked OTP. No backend hit.
- **Trace capture on failure** — automatically saved to `bin/Debug/net8.0/traces/`
- **End-to-end pass** — the test runs visibly in Chrome and turns green

The full original Selenium scenario drives an end-to-end registration → login → OTP submission flow that depends on backend integration (BlueBrown microservices, MongoDB UserPool, real OTP endpoints). Reproducing that fidelity here would require porting the whole infrastructure stack — which is **explicitly out of scope** for the showcase. The point is the **code shape and developer experience**, not feature parity.

The comparison isn't "will this test pass with all the same fidelity?" — it's "which of these codebases would you rather maintain?"
