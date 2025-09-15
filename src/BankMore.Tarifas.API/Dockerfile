FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Shared/BankMore.Shared.csproj", "src/BankMore.Shared/"]
COPY ["src/BankMore.Tarifas.Domain/BankMore.Tarifas.Domain.csproj", "src/BankMore.Tarifas.Domain/"]
COPY ["src/BankMore.Tarifas.Infrastructure/BankMore.Tarifas.Infrastructure.csproj", "src/BankMore.Tarifas.Infrastructure/"]
COPY ["src/BankMore.Tarifas.API/BankMore.Tarifas.API.csproj", "src/BankMore.Tarifas.API/"]
RUN dotnet restore "src/BankMore.Tarifas.API/BankMore.Tarifas.API.csproj"
COPY . .
WORKDIR "/src/src/BankMore.Tarifas.API"
RUN dotnet build "BankMore.Tarifas.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Tarifas.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Tarifas.API.dll"]
