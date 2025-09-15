using BankMore.Tarifas.Domain.Entities;
using BankMore.Tarifas.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.Tarifas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TarifasController : ControllerBase
{
    private readonly ITarifaRepository _tarifaRepository;
    private readonly ILogger<TarifasController> _logger;

    public TarifasController(ITarifaRepository tarifaRepository, ILogger<TarifasController> logger)
    {
        _tarifaRepository = tarifaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Consulta tarifas aplicadas (implementação básica)
    /// </summary>
    /// <returns>Lista de tarifas</returns>
    [HttpGet("consultar")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> ConsultarTarifas()
    {
        try
        {
            // Implementação básica - em produção seria mais complexa
            _logger.LogInformation("Consultando tarifas aplicadas");
            
            // Por enquanto, retorna uma resposta mockada
            // TODO: Implementar consulta real no banco de dados
            var response = new
            {
                message = "API de Tarifas funcionando",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                status = "Ativo",
                // Mock de dados para demonstração
                tarifas = new[]
                {
                    new { tipo = "Transferência", valor = 2.50m, data = DateTime.UtcNow.AddDays(-1).ToString("dd/MM/yyyy") },
                    new { tipo = "Saque", valor = 1.00m, data = DateTime.UtcNow.AddDays(-2).ToString("dd/MM/yyyy") }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar tarifas");
            return BadRequest(new ErrorResponse
            {
                Error = "Erro interno na consulta de tarifas",
                ErrorType = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Aplica uma tarifa (endpoint adicional)
    /// </summary>
    /// <param name="request">Dados da tarifa</param>
    /// <returns>Resultado da aplicação</returns>
    [HttpPost("aplicar")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> AplicarTarifa([FromBody] AplicarTarifaRequest request)
    {
        try
        {
            // Validação básica
            if (string.IsNullOrEmpty(request.IdContaCorrente) || request.Valor <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Dados inválidos para aplicação de tarifa",
                    ErrorType = "INVALID_INPUT"
                });
            }

            // Gerar ID único para a tarifa
            var idTarifa = Guid.NewGuid().ToString();
            
            // Criar entidade tarifa
            var tarifa = new Tarifa(idTarifa, request.IdContaCorrente, request.Valor);
            
            // Salvar no banco
            await _tarifaRepository.SalvarAsync(tarifa);

            _logger.LogInformation("Tarifa aplicada: {IdTarifa} - Conta: {IdConta} - Valor: {Valor}", 
                idTarifa, request.IdContaCorrente, request.Valor);

            var response = new
            {
                idTarifa = idTarifa,
                idContaCorrente = request.IdContaCorrente,
                valor = request.Valor,
                dataAplicacao = tarifa.DataMovimento.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                status = "Aplicada"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aplicar tarifa");
            return BadRequest(new ErrorResponse
            {
                Error = "Erro interno na aplicação de tarifa",
                ErrorType = "INTERNAL_ERROR"
            });
        }
    }
}

public class AplicarTarifaRequest
{
    public string IdContaCorrente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
}
