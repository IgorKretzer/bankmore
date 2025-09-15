FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Shared/BankMore.Shared.csproj", "src/BankMore.Shared/"]
COPY ["src/BankMore.ContaCorrente.Domain/BankMore.ContaCorrente.Domain.csproj", "src/BankMore.ContaCorrente.Domain/"]
COPY ["src/BankMore.ContaCorrente.Infrastructure/BankMore.ContaCorrente.Infrastructure.csproj", "src/BankMore.ContaCorrente.Infrastructure/"]
COPY ["src/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj", "src/BankMore.ContaCorrente.API/"]
RUN dotnet restore "src/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj"
COPY . .
WORKDIR "/src/src/BankMore.ContaCorrente.API"
RUN dotnet build "BankMore.ContaCorrente.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.ContaCorrente.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.ContaCorrente.API.dll"]
