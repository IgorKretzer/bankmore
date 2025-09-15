using BankMore.Shared.Common;
using System.Net;
using System.Text.Json;

namespace BankMore.ContaCorrente.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log específico para operações bancárias
            _logger.LogError(ex, "Erro não tratado na API de Conta Corrente - Path: {Path}", context.Request.Path);
            await ProcessarErroBancario(context, ex);
        }
    }

    private async Task ProcessarErroBancario(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Resposta mais específica para operações bancárias
        var response = new
        {
            error = "Erro interno no processamento da operação bancária",
            errorType = ErrorTypes.INTERNAL_ERROR,
            details = exception.Message,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            // Adicionar informações específicas do contexto bancário
            operation = ObterOperacaoAtual(context.Request.Path)
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private string ObterOperacaoAtual(string path)
    {
        // Identificar qual operação estava sendo executada
        if (path.Contains("/cadastrar"))
            return "Cadastro de Conta";
        if (path.Contains("/login"))
            return "Login";
        if (path.Contains("/movimentar"))
            return "Movimentação de Conta";
        if (path.Contains("/saldo"))
            return "Consulta de Saldo";
        if (path.Contains("/inativar"))
            return "Inativação de Conta";
        
        return "Operação Desconhecida";
    }
}
