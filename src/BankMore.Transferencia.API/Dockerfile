FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Shared/BankMore.Shared.csproj", "src/BankMore.Shared/"]
COPY ["src/BankMore.Transferencia.Domain/BankMore.Transferencia.Domain.csproj", "src/BankMore.Transferencia.Domain/"]
COPY ["src/BankMore.Transferencia.Infrastructure/BankMore.Transferencia.Infrastructure.csproj", "src/BankMore.Transferencia.Infrastructure/"]
COPY ["src/BankMore.Transferencia.API/BankMore.Transferencia.API.csproj", "src/BankMore.Transferencia.API/"]
RUN dotnet restore "src/BankMore.Transferencia.API/BankMore.Transferencia.API.csproj"
COPY . .
WORKDIR "/src/src/BankMore.Transferencia.API"
RUN dotnet build "BankMore.Transferencia.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Transferencia.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Transferencia.API.dll"]
