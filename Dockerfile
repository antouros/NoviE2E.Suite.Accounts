FROM mcr.microsoft.com/playwright/dotnet:v1.49.0-noble AS build

WORKDIR /app

COPY NoviE2E.Suite.Accounts.sln .
COPY NoviE2E.Suite.Accounts.Tests/ NoviE2E.Suite.Accounts.Tests/

RUN dotnet restore
RUN dotnet build --no-restore

ENTRYPOINT ["dotnet", "test", "--no-build", "--logger", "trx;LogFileName=results.trx"]
