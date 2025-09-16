using BankMore.Shared.Common;
using System.Net;
using System.Text.Json;

namespace BankMore.ContaCorrente.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado na API de Conta Corrente - Path: {Path}", context.Request.Path);
            await ProcessarErroBancario(context, ex);
        }
    }

    private async Task ProcessarErroBancario(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Erro interno no processamento da operação bancária",
            errorType = ErrorTypes.INTERNALERROR,
            details = exception.Message,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
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
