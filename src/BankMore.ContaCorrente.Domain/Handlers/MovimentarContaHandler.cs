using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, Result>
{
    private readonly IContaCorrenteRepository contaRepository;
    private readonly IMovimentoRepository movimentoRepository;
    private readonly IIdempotenciaRepository idempotenciaRepository;

    public MovimentarContaHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        IIdempotenciaRepository idempotenciaRepository)
    {
        this.contaRepository = contaRepository;
        this.movimentoRepository = movimentoRepository;
        this.idempotenciaRepository = idempotenciaRepository;
    }

    public async Task<Result> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        var resultadoAnterior = await VerificarIdempotencia(request.IdRequisicao);
        if (resultadoAnterior != null)
        {
            return resultadoAnterior; // Já processado
        }

        var validacaoResult = await ValidarRequisicao(request);
        if (validacaoResult.IsFailure)
        {
            await RegistrarFalhaIdempotencia(request.IdRequisicao, validacaoResult.Error, validacaoResult.ErrorType);
            return validacaoResult;
        }

        var idContaDestino = await DeterminarContaDestino(request);
        if (string.IsNullOrEmpty(idContaDestino))
        {
            await RegistrarFalhaIdempotencia(request.IdRequisicao, "Conta de destino inválida", "INVALIDACCOUNT");
            return Result.Failure("Conta de destino inválida", ErrorTypes.INVALIDACCOUNT);
        }

        var conta = await contaRepository.ObterPorIdAsync(idContaDestino);
        if (conta == null || !conta.Ativo)
        {
            var erro = conta == null ? "Conta não encontrada" : "Conta inativa";
            var tipoErro = conta == null ? ErrorTypes.INVALIDACCOUNT : ErrorTypes.INACTIVEACCOUNT;
            await RegistrarFalhaIdempotencia(request.IdRequisicao, erro, tipoErro);
            return Result.Failure(erro, tipoErro);
        }

        if (request.TipoMovimento == 'D')
        {
            var saldoSuficiente = await VerificarSaldoSuficiente(idContaDestino, request.Valor);
            if (!saldoSuficiente)
            {
                await RegistrarFalhaIdempotencia(request.IdRequisicao, "Saldo insuficiente", "INSUFFICIENTFUNDS");
                return Result.Failure("Saldo insuficiente", ErrorTypes.INSUFFICIENTFUNDS);
            }
        }

        await ProcessarMovimentacao(request, idContaDestino);

        await RegistrarSucessoIdempotencia(request.IdRequisicao, request.TipoMovimento, request.Valor);

        return Result.Success();
    }

    private async Task<Result?> VerificarIdempotencia(string idRequisicao)
    {
        var resultadoAnterior = await idempotenciaRepository.ObterResultadoAsync(idRequisicao);
        if (!string.IsNullOrEmpty(resultadoAnterior))
        {
            return Result.Success(); // Operação já foi processada
        }
        return null;
    }

    private Task<Result> ValidarRequisicao(MovimentarContaCommand request)
    {
        if (!ValidationHelper.IsValidValue(request.Valor))
        {
            return Task.FromResult(Result.Failure("Valor deve ser positivo", ErrorTypes.INVALIDVALUE));
        }

        if (request.TipoMovimento != 'C' && request.TipoMovimento != 'D')
        {
            return Task.FromResult(Result.Failure("Tipo de movimento deve ser C (Crédito) ou D (Débito)", ErrorTypes.INVALIDTYPE));
        }

        return Task.FromResult(Result.Success());
    }

    private async Task<string?> DeterminarContaDestino(MovimentarContaCommand request)
    {
        if (!request.NumeroConta.HasValue)
        {
            return request.IdContaCorrente;
        }

        var contaDestino = await contaRepository.ObterPorNumeroAsync(request.NumeroConta.Value);
        if (contaDestino == null)
        {
            return null;
        }

        if (contaDestino.IdContaCorrente != request.IdContaCorrente && request.TipoMovimento != 'C')
        {
            return null; // Vai gerar erro na validação
        }

        return contaDestino.IdContaCorrente;
    }

    private async Task<bool> VerificarSaldoSuficiente(string idConta, decimal valor)
    {
        var movimentos = await movimentoRepository.ObterPorContaAsync(idConta);
        var saldo = ValueObjects.Saldo.CalcularSaldo(movimentos);
        
        return saldo.Valor >= valor;
    }

    private async Task ProcessarMovimentacao(MovimentarContaCommand request, string idContaDestino)
    {
        var movimento = request.TipoMovimento == 'C' 
            ? Movimento.CriarCredito(request.IdRequisicao, idContaDestino, request.Valor)
            : Movimento.CriarDebito(request.IdRequisicao, idContaDestino, request.Valor);

        await movimentoRepository.SalvarAsync(movimento);
    }

    private async Task RegistrarFalhaIdempotencia(string idRequisicao, string erro, string tipoErro)
    {
        await idempotenciaRepository.SalvarAsync(idRequisicao, erro, tipoErro);
    }

    private async Task RegistrarSucessoIdempotencia(string idRequisicao, char tipoMovimento, decimal valor)
    {
        var mensagem = $"Movimento {tipoMovimento} de {valor:C} processado com sucesso";
        await idempotenciaRepository.SalvarAsync(idRequisicao, mensagem, "SUCCESS");
    }
}
