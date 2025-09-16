using BankMore.Shared.Common;
using System.Net;
using System.Text.Json;

namespace BankMore.Transferencia.API.Middleware;

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
            _logger.LogError(ex, "Falha no processamento de transferência - RequestId: {RequestId}", 
                context.TraceIdentifier);
            await TratarErroTransferencia(context, ex);
        }
    }

    private async Task TratarErroTransferencia(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Falha no processamento da transferência",
            errorType = ErrorTypes.TRANSFER_FAILED,
            details = exception.Message,
            requestId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow.ToString("O"),
            transferOperation = IdentificarOperacaoTransferencia(context.Request.Path)
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private string IdentificarOperacaoTransferencia(string path)
    {
        if (path.Contains("/efetuar"))
            return "Efetuação de Transferência";
        if (path.Contains("/consultar"))
            return "Consulta de Transferência";
        if (path.Contains("/estornar"))
            return "Estorno de Transferência";
        
        return "Operação de Transferência Desconhecida";
    }
}
