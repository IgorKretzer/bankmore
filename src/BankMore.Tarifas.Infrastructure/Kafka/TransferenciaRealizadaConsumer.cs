/*
using BankMore.Tarifas.Domain.Entities;
using BankMore.Tarifas.Domain.Events;
using BankMore.Tarifas.Domain.Interfaces;
using KafkaFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BankMore.Tarifas.Infrastructure.Kafka;

public class TransferenciaRealizadaConsumer : IMessageHandler<TransferenciaRealizadaEvent>
{
    private readonly ITarifaRepository tarifaRepository;
    private readonly IMessageProducer messageProducer;
    private readonly IConfiguration configuration;
    private readonly ILogger<TransferenciaRealizadaConsumer> logger;
    private readonly decimal valorTarifa;

    public TransferenciaRealizadaConsumer(
        ITarifaRepository tarifaRepository,
        IMessageProducer messageProducer,
        IConfiguration configuration,
        ILogger<TransferenciaRealizadaConsumer> logger)
    {
        this.tarifaRepository = tarifaRepository;
        this.messageProducer = messageProducer;
        this.configuration = configuration;
        this.logger = logger;
        valorTarifa = configuration.GetValue<decimal>("Tarifas:ValorTransferencia", 2.00m);
    }

    public async Task Handle(IMessageContext context, TransferenciaRealizadaEvent message)
    {
        try
        {
            logger.LogInformation("Processando transferência realizada: {IdRequisicao}", message.IdRequisicao);

            var tarifa = new Tarifa(
                Guid.NewGuid().ToString(),
                message.IdContaCorrente,
                valorTarifa
            );

            await tarifaRepository.SalvarAsync(tarifa);

            var tarifaAplicadaEvent = new TarifaAplicadaEvent
            {
                IdContaCorrente = message.IdContaCorrente,
                ValorTarifado = valorTarifa
            };

            await messageProducer.ProduceAsync("tarifas-realizadas", tarifaAplicadaEvent);

            logger.LogInformation("Tarifa aplicada com sucesso: {IdContaCorrente} - {Valor}", 
                message.IdContaCorrente, valorTarifa);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar transferência realizada: {IdRequisicao}", message.IdRequisicao);
            throw;
        }
    }
}
*/
