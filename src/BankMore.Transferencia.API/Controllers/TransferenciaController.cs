using BankMore.Transferencia.Domain.Commands;
using BankMore.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankMore.Transferencia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferenciaController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ILogger<TransferenciaController> logger;

    public TransferenciaController(IMediator mediator, ILogger<TransferenciaController> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    /// <summary>
    /// Efetua transferência entre contas da mesma instituição
    /// </summary>
    /// <param name="request">Dados da transferência</param>
    /// <returns>Sucesso</returns>
    [HttpPost("efetuar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<IActionResult> EfetuarTransferencia([FromBody] EfetuarTransferenciaRequest request)
    {
        var contaId = User.FindFirst("contaId")?.Value;
        if (string.IsNullOrEmpty(contaId))
        {
            return Forbid();
        }

        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var command = new EfetuarTransferenciaCommand
        {
            IdRequisicao = request.IdRequisicao,
            IdContaCorrenteOrigem = contaId,
            NumeroContaDestino = request.NumeroContaDestino,
            Valor = request.Valor,
            Token = token
        };

        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.Error,
                ErrorType = result.ErrorType
            });
        }

        return NoContent();
    }
}

public class EfetuarTransferenciaRequest
{
    public string IdRequisicao { get; set; } = string.Empty;
    public int NumeroContaDestino { get; set; }
    public decimal Valor { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
}
