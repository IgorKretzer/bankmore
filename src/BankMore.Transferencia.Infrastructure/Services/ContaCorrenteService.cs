using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Shared.Common;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankMore.Transferencia.Infrastructure.Services;

public class ContaCorrenteService : IContaCorrenteService
{
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly ILogger<ContaCorrenteService> logger;

    public ContaCorrenteService(HttpClient httpClient, IConfiguration configuration, ILogger<ContaCorrenteService> logger)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<Result> RealizarDebitoAsync(string idRequisicao, string idContaCorrente, decimal valor, string token)
    {
        try
        {
            var request = new
            {
                IdRequisicao = idRequisicao,
                Valor = valor,
                TipoMovimento = 'D'
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = configuration["ContaCorrenteApi:BaseUrl"] ?? "http://localhost:5001";
            var response = await httpClient.PostAsync($"{baseUrl}/api/ContaCorrente/movimentar", content);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Erro ao realizar débito: {Error}", errorContent);

            return Result.Failure("Falha ao realizar débito", ErrorTypes.TRANSFERFAILED);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao realizar débito");
            return Result.Failure($"Erro interno: {ex.Message}", ErrorTypes.INTERNALERROR);
        }
    }

    public async Task<Result> RealizarCreditoAsync(string idRequisicao, int numeroConta, decimal valor, string token)
    {
        try
        {
            var request = new
            {
                IdRequisicao = idRequisicao,
                NumeroConta = numeroConta,
                Valor = valor,
                TipoMovimento = 'C'
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = configuration["ContaCorrenteApi:BaseUrl"] ?? "http://localhost:5001";
            var response = await httpClient.PostAsync($"{baseUrl}/api/ContaCorrente/movimentar", content);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Erro ao realizar crédito: {Error}", errorContent);

            return Result.Failure("Falha ao realizar crédito", ErrorTypes.TRANSFERFAILED);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao realizar crédito");
            return Result.Failure($"Erro interno: {ex.Message}", ErrorTypes.INTERNALERROR);
        }
    }
}
