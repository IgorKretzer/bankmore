using BankMore.Transferencia.Domain.Commands;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;
using BankMore.Transferencia.Domain.Events;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.Transferencia.Domain.Handlers;

public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaCommand, Result>
{
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IContaCorrenteService _contaCorrenteService;
    private readonly IMessageProducer _messageProducer;

    public EfetuarTransferenciaHandler(
        ITransferenciaRepository transferenciaRepository,
        IContaCorrenteService contaCorrenteService,
        IMessageProducer messageProducer)
    {
        _transferenciaRepository = transferenciaRepository;
        _contaCorrenteService = contaCorrenteService;
        _messageProducer = messageProducer;
    }

    public async Task<Result> Handle(EfetuarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        if (!ValidationHelper.IsValidValue(request.Valor))
        {
            return Result.Failure("Valor deve ser positivo", ErrorTypes.INVALID_VALUE);
        }

        try
        {
            var debitoResult = await _contaCorrenteService.RealizarDebitoAsync(
                request.IdRequisicao + "_debito",
                request.IdContaCorrenteOrigem,
                request.Valor,
                request.Token);

            if (debitoResult.IsFailure)
            {
                return Result.Failure($"Falha no débito: {debitoResult.Error}", ErrorTypes.TRANSFER_FAILED);
            }

            var creditoResult = await _contaCorrenteService.RealizarCreditoAsync(
                request.IdRequisicao + "_credito",
                request.NumeroContaDestino,
                request.Valor,
                request.Token);

            if (creditoResult.IsFailure)
            {
                // await _contaCorrenteService.RealizarCreditoAsync(
                //     request.IdRequisicao + "_estorno",
                //     int.Parse(request.IdContaCorrenteOrigem), // Assumindo que IdContaCorrenteOrigem contém o número
                //     request.Valor,
                //     request.Token);

                return Result.Failure($"Falha no crédito: {creditoResult.Error}", ErrorTypes.TRANSFER_FAILED);
            }

            var transferencia = new TransferenciaEntity(
                request.IdRequisicao,
                request.IdContaCorrenteOrigem,
                request.NumeroContaDestino.ToString(), // Assumindo que precisamos do ID da conta destino
                request.Valor);

            await _transferenciaRepository.SalvarAsync(transferencia);

            var evento = new TransferenciaRealizadaEvent
            {
                IdRequisicao = request.IdRequisicao,
                IdContaCorrente = request.IdContaCorrenteOrigem
            };

            await _messageProducer.ProduceAsync("transferencias-realizadas", evento);

            return Result.Success();
        }
        catch (Exception ex)
        {
            try
            {
                await _contaCorrenteService.RealizarCreditoAsync(
                    request.IdRequisicao + "_estorno_erro",
                    int.Parse(request.IdContaCorrenteOrigem),
                    request.Valor,
                    request.Token);
            }
            catch
            {
            }

            return Result.Failure($"Erro interno: {ex.Message}", ErrorTypes.INTERNAL_ERROR);
        }
    }
}
