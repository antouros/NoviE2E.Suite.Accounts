# NoviE2E.Suite.Accounts

Playwright test suite for the **Accounts vertical** — built as a standalone alternative to the QA.Novi Selenium monorepo.

**Stack:** Microsoft Playwright · Reqnroll (BDD/Gherkin) · NUnit · .NET 8

---

## Setup

```powershell
# Clone and enter the repo
git clone https://github.com/antouros/NoviE2E.Suite.Accounts
cd NoviE2E.Suite.Accounts

# Copy the credentials template and fill in your values
copy NoviE2E.Suite.Accounts.Tests\appsettings.template.json `
     NoviE2E.Suite.Accounts.Tests\appsettings.json

# Restore, build, install browser
dotnet restore
dotnet build
.\NoviE2E.Suite.Accounts.Tests\bin\Debug\net8.0\playwright.ps1 install chromium

# Run
dotnet test
```

---

## Debugging failures with Trace Viewer

On failure, a trace is auto-saved to:

```
NoviE2E.Suite.Accounts.Tests/bin/Debug/net8.0/traces/<ScenarioName>.zip
```

Open it at **https://trace.playwright.dev** — fully local, zero data leaves your machine.

In CI, the trace ZIP is uploaded as a GitHub Actions artifact. Download it from the workflow run and drop it into trace.playwright.dev.

---

## CI

GitHub Actions workflow: `.github/workflows/run-tests.yml`

Add two repository secrets before the first run:
- `NOVIBET_USERNAME`
- `NOVIBET_PASSWORD`

The test runs in a Docker container (`mcr.microsoft.com/playwright/dotnet`) — no browser installation needed in CI.
