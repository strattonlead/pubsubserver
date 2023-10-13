FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet nuget add source --username strattonlead --password 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 --store-password-in-clear-text --name createif-labs "https://nuget.pkg.github.com/strattonlead/index.json"

FROM build AS publish
RUN dotnet publish "PubSubServer.csproj" -c Debug -o /app/publish

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PubSubServer.dll"]