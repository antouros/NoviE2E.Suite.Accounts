# NoviE2E.Suite.Accounts

Playwright test suite for the **Accounts vertical** — built as a standalone alternative to the QA.Novi Selenium monorepo.

**Stack:** Microsoft Playwright · Reqnroll (BDD/Gherkin) · NUnit · .NET 8

---

## Setup (one-time)

```powershell
git clone https://github.com/antouros/NoviE2E.Suite.Accounts
cd NoviE2E.Suite.Accounts
dotnet restore
dotnet build
.\NoviE2E.Suite.Accounts.Tests\bin\Debug\net8.0\playwright.ps1 install chromium
```

## Run tests

```powershell
dotnet test
```

> `dotnet build` and `playwright.ps1 install` are one-time setup steps — you don't need to repeat them every time you run tests.

---

## Debugging failures with Trace Viewer

On failure, a trace is auto-saved to:

```
NoviE2E.Suite.Accounts.Tests/bin/Debug/net8.0/traces/<ScenarioName>.zip
```

**In CI:** click the **🔍 Open Trace Viewer** link in the job summary — one click, no download.

**Locally:**
```powershell
.\NoviE2E.Suite.Accounts.Tests\bin\Debug\net8.0\playwright.ps1 show-trace traces\<ScenarioName>.zip
```

> If a colleague gets a *"Local Network Access"* error on the CI link, fix it in Chrome:
> `chrome://flags/#block-insecure-private-network-requests` → **Disabled** → relaunch Chrome

---

## CI

GitHub Actions: `.github/workflows/run-tests.yml`

Runs in a Docker container (`mcr.microsoft.com/playwright/dotnet`) — no manual browser install needed.
