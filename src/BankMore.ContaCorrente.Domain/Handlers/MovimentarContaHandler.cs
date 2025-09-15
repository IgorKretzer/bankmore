using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, Result>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;

    public MovimentarContaHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository,
        IIdempotenciaRepository idempotenciaRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _idempotenciaRepository = idempotenciaRepository;
    }

    public async Task<Result> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        // Primeiro, verificar se já processamos esta requisição (idempotência)
        var resultadoAnterior = await VerificarIdempotencia(request.IdRequisicao);
        if (resultadoAnterior != null)
        {
            return resultadoAnterior; // Já processado
        }

        // Validar dados da requisição
        var validacaoResult = await ValidarRequisicao(request);
        if (validacaoResult.IsFailure)
        {
            await RegistrarFalhaIdempotencia(request.IdRequisicao, validacaoResult.Error, validacaoResult.ErrorType);
            return validacaoResult;
        }

        // Determinar qual conta será movimentada
        var idContaDestino = await DeterminarContaDestino(request);
        if (string.IsNullOrEmpty(idContaDestino))
        {
            await RegistrarFalhaIdempotencia(request.IdRequisicao, "Conta de destino inválida", "INVALID_ACCOUNT");
            return Result.Failure("Conta de destino inválida", ErrorTypes.INVALID_ACCOUNT);
        }

        // Buscar e validar conta de destino
        var conta = await _contaRepository.ObterPorIdAsync(idContaDestino);
        if (conta == null || !conta.Ativo)
        {
            var erro = conta == null ? "Conta não encontrada" : "Conta inativa";
            var tipoErro = conta == null ? ErrorTypes.INVALID_ACCOUNT : ErrorTypes.INACTIVE_ACCOUNT;
            await RegistrarFalhaIdempotencia(request.IdRequisicao, erro, tipoErro);
            return Result.Failure(erro, tipoErro);
        }

        // Verificar saldo se for débito
        if (request.TipoMovimento == 'D')
        {
            var saldoSuficiente = await VerificarSaldoSuficiente(idContaDestino, request.Valor);
            if (!saldoSuficiente)
            {
                await RegistrarFalhaIdempotencia(request.IdRequisicao, "Saldo insuficiente", "INSUFFICIENT_FUNDS");
                return Result.Failure("Saldo insuficiente", ErrorTypes.INSUFFICIENT_FUNDS);
            }
        }

        // Processar a movimentação
        await ProcessarMovimentacao(request, idContaDestino);

        // Registrar sucesso na idempotência
        await RegistrarSucessoIdempotencia(request.IdRequisicao, request.TipoMovimento, request.Valor);

        return Result.Success();
    }

    private async Task<Result?> VerificarIdempotencia(string idRequisicao)
    {
        var resultadoAnterior = await _idempotenciaRepository.ObterResultadoAsync(idRequisicao);
        if (!string.IsNullOrEmpty(resultadoAnterior))
        {
            return Result.Success(); // Operação já foi processada
        }
        return null;
    }

    private Task<Result> ValidarRequisicao(MovimentarContaCommand request)
    {
        // Validar valor - deve ser positivo
        if (!ValidationHelper.IsValidValue(request.Valor))
        {
            return Task.FromResult(Result.Failure("Valor deve ser positivo", ErrorTypes.INVALID_VALUE));
        }

        // Validar tipo de movimento - só C ou D por enquanto
        if (request.TipoMovimento != 'C' && request.TipoMovimento != 'D')
        {
            return Task.FromResult(Result.Failure("Tipo de movimento deve ser C (Crédito) ou D (Débito)", ErrorTypes.INVALID_TYPE));
        }

        return Task.FromResult(Result.Success());
    }

    private async Task<string?> DeterminarContaDestino(MovimentarContaCommand request)
    {
        // Se não especificou número, usa a conta do token
        if (!request.NumeroConta.HasValue)
        {
            return request.IdContaCorrente;
        }

        // Buscar conta por número
        var contaDestino = await _contaRepository.ObterPorNumeroAsync(request.NumeroConta.Value);
        if (contaDestino == null)
        {
            return null;
        }

        // Se for conta diferente, só permite crédito (regra de negócio)
        if (contaDestino.IdContaCorrente != request.IdContaCorrente && request.TipoMovimento != 'C')
        {
            return null; // Vai gerar erro na validação
        }

        return contaDestino.IdContaCorrente;
    }

    private async Task<bool> VerificarSaldoSuficiente(string idConta, decimal valor)
    {
        // Buscar movimentos da conta para calcular saldo
        var movimentos = await _movimentoRepository.ObterPorContaAsync(idConta);
        var saldo = ValueObjects.Saldo.CalcularSaldo(movimentos);
        
        return saldo.Valor >= valor;
    }

    private async Task ProcessarMovimentacao(MovimentarContaCommand request, string idContaDestino)
    {
        // Criar movimento baseado no tipo
        var movimento = request.TipoMovimento == 'C' 
            ? Movimento.CriarCredito(request.IdRequisicao, idContaDestino, request.Valor)
            : Movimento.CriarDebito(request.IdRequisicao, idContaDestino, request.Valor);

        // Persistir no banco
        await _movimentoRepository.SalvarAsync(movimento);
    }

    private async Task RegistrarFalhaIdempotencia(string idRequisicao, string erro, string tipoErro)
    {
        await _idempotenciaRepository.SalvarAsync(idRequisicao, erro, tipoErro);
    }

    private async Task RegistrarSucessoIdempotencia(string idRequisicao, char tipoMovimento, decimal valor)
    {
        var mensagem = $"Movimento {tipoMovimento} de {valor:C} processado com sucesso";
        await _idempotenciaRepository.SalvarAsync(idRequisicao, mensagem, "SUCCESS");
    }
}
