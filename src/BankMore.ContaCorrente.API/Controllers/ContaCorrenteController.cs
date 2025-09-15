using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Queries;
using BankMore.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankMore.ContaCorrente.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaCorrenteController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContaCorrenteController> _logger;

    public ContaCorrenteController(IMediator mediator, ILogger<ContaCorrenteController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra uma nova conta corrente
    /// </summary>
    /// <param name="request">Dados para cadastro da conta</param>
    /// <returns>Número da conta criada</returns>
    [HttpPost("cadastrar")]
    [ProducesResponseType(typeof(CadastrarContaResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> CadastrarConta([FromBody] CadastrarContaRequest request)
    {
        // TODO: Adicionar validação de rate limiting para evitar spam de cadastros
        // var rateLimitKey = $"cadastro_{Request.HttpContext.Connection.RemoteIpAddress}";
        
        var command = new CadastrarContaCommand
        {
            Cpf = request.Cpf,
            Nome = request.Nome,
            Senha = request.Senha
        };

        // Debug: log da requisição (remover em produção)
        _logger.LogDebug("Processando cadastro para CPF: {Cpf}", request.Cpf);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            // Log do erro para monitoramento
            _logger.LogWarning("Falha no cadastro: {Error} - CPF: {Cpf}", result.Error, request.Cpf);
            
            return BadRequest(new ErrorResponse
            {
                Error = result.Error,
                ErrorType = result.ErrorType
            });
        }

        // Log de sucesso
        _logger.LogInformation("Conta cadastrada com sucesso: {NumeroConta}", result.Value.NumeroConta);

        return Ok(result.Value);
    }

    /// <summary>
    /// Realiza login na conta corrente
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <returns>Token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Validação básica antes de processar
        if (string.IsNullOrEmpty(request.Identificacao) || string.IsNullOrEmpty(request.Senha))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Identificação e senha são obrigatórios",
                ErrorType = "INVALID_INPUT"
            });
        }

        var command = new LoginCommand
        {
            Identificacao = request.Identificacao,
            Senha = request.Senha
        };

        // Log de tentativa de login (sem senha por segurança)
        _logger.LogInformation("Tentativa de login para: {Identificacao}", request.Identificacao);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            // Log de falha no login
            _logger.LogWarning("Falha no login: {Error} - Identificação: {Identificacao}", 
                result.Error, request.Identificacao);
            
            return Unauthorized(new ErrorResponse
            {
                Error = result.Error,
                ErrorType = result.ErrorType
            });
        }

        // Log de sucesso no login
        _logger.LogInformation("Login realizado com sucesso para conta: {NumeroConta}", 
            result.Value.NumeroConta);

        return Ok(result.Value);
    }

    /// <summary>
    /// Inativa uma conta corrente
    /// </summary>
    /// <param name="request">Dados para inativação</param>
    /// <returns>Sucesso</returns>
    [HttpPost("inativar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<IActionResult> InativarConta([FromBody] InativarContaRequest request)
    {
        var contaId = User.FindFirst("contaId")?.Value;
        if (string.IsNullOrEmpty(contaId))
        {
            return Forbid();
        }

        var command = new InativarContaCommand
        {
            IdContaCorrente = contaId,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);

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

    /// <summary>
    /// Realiza movimentação na conta (depósito ou saque)
    /// </summary>
    /// <param name="request">Dados da movimentação</param>
    /// <returns>Sucesso</returns>
    [HttpPost("movimentar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<IActionResult> MovimentarConta([FromBody] MovimentarContaRequest request)
    {
        var contaId = User.FindFirst("contaId")?.Value;
        if (string.IsNullOrEmpty(contaId))
        {
            return Forbid();
        }

        var command = new MovimentarContaCommand
        {
            IdRequisicao = request.IdRequisicao,
            IdContaCorrente = contaId,
            NumeroConta = request.NumeroConta,
            Valor = request.Valor,
            TipoMovimento = request.TipoMovimento
        };

        var result = await _mediator.Send(command);

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

    /// <summary>
    /// Consulta o saldo da conta corrente
    /// </summary>
    /// <returns>Saldo da conta</returns>
    [HttpGet("saldo")]
    [Authorize]
    [ProducesResponseType(typeof(ConsultarSaldoResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<IActionResult> ConsultarSaldo()
    {
        var contaId = User.FindFirst("contaId")?.Value;
        if (string.IsNullOrEmpty(contaId))
        {
            return Forbid();
        }

        var query = new ConsultarSaldoQuery
        {
            IdContaCorrente = contaId
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.Error,
                ErrorType = result.ErrorType
            });
        }

        return Ok(result.Value);
    }
}

// DTOs para requests
public class CadastrarContaRequest
{
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Identificacao { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class InativarContaRequest
{
    public string Senha { get; set; } = string.Empty;
}

public class MovimentarContaRequest
{
    public string IdRequisicao { get; set; } = string.Empty;
    public int? NumeroConta { get; set; }
    public decimal Valor { get; set; }
    public char TipoMovimento { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
}
