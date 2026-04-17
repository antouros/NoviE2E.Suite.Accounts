FROM mcr.microsoft.com/playwright/dotnet:v1.49.0-noble AS build

WORKDIR /app

COPY NoviE2E.Suite.Accounts.sln .
COPY NoviE2E.Suite.Accounts.Tests/ NoviE2E.Suite.Accounts.Tests/

RUN dotnet restore
RUN dotnet build --no-restore

# appsettings.json is injected at runtime via env vars or volume mount — never baked into the image.
# Copy the template so the container can self-document required config.
COPY NoviE2E.Suite.Accounts.Tests/appsettings.template.json NoviE2E.Suite.Accounts.Tests/appsettings.template.json

ENTRYPOINT ["dotnet", "test", "--no-build", "--logger", "trx;LogFileName=results.trx"]
