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
    private readonly ITransferenciaRepository transferenciaRepository;
    private readonly IContaCorrenteService contaCorrenteService;
    private readonly IMessageProducer messageProducer;

    public EfetuarTransferenciaHandler(
        ITransferenciaRepository transferenciaRepository,
        IContaCorrenteService contaCorrenteService,
        IMessageProducer messageProducer)
    {
        this.transferenciaRepository = transferenciaRepository;
        this.contaCorrenteService = contaCorrenteService;
        this.messageProducer = messageProducer;
    }

    public async Task<Result> Handle(EfetuarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        if (!ValidationHelper.IsValidValue(request.Valor))
        {
            return Result.Failure("Valor deve ser positivo", ErrorTypes.INVALIDVALUE);
        }

        try
        {
            var debitoResult = await contaCorrenteService.RealizarDebitoAsync(
                request.IdRequisicao + "debito",
                request.IdContaCorrenteOrigem,
                request.Valor,
                request.Token);

            if (debitoResult.IsFailure)
            {
                return Result.Failure($"Falha no débito: {debitoResult.Error}", ErrorTypes.TRANSFERFAILED);
            }

            var creditoResult = await contaCorrenteService.RealizarCreditoAsync(
                request.IdRequisicao + "credito",
                request.NumeroContaDestino,
                request.Valor,
                request.Token);

            if (creditoResult.IsFailure)
            {
                // await contaCorrenteService.RealizarCreditoAsync(
                //     request.IdRequisicao + "estorno",
                //     int.Parse(request.IdContaCorrenteOrigem), // Assumindo que IdContaCorrenteOrigem contém o número
                //     request.Valor,
                //     request.Token);

                return Result.Failure($"Falha no crédito: {creditoResult.Error}", ErrorTypes.TRANSFERFAILED);
            }

            var transferencia = new TransferenciaEntity(
                request.IdRequisicao,
                request.IdContaCorrenteOrigem,
                request.NumeroContaDestino.ToString(), // Assumindo que precisamos do ID da conta destino
                request.Valor);

            await transferenciaRepository.SalvarAsync(transferencia);

            var evento = new TransferenciaRealizadaEvent
            {
                IdRequisicao = request.IdRequisicao,
                IdContaCorrente = request.IdContaCorrenteOrigem
            };

            await messageProducer.ProduceAsync("transferencias-realizadas", evento);

            return Result.Success();
        }
        catch (Exception ex)
        {
            try
            {
                await contaCorrenteService.RealizarCreditoAsync(
                    request.IdRequisicao + "estornoerro",
                    int.Parse(request.IdContaCorrenteOrigem),
                    request.Valor,
                    request.Token);
            }
            catch
            {
            }

            return Result.Failure($"Erro interno: {ex.Message}", ErrorTypes.INTERNALERROR);
        }
    }
}
