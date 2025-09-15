// Kafka Consumer comentado por enquanto
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
    private readonly ITarifaRepository _tarifaRepository;
    private readonly IMessageProducer _messageProducer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TransferenciaRealizadaConsumer> _logger;
    private readonly decimal _valorTarifa;

    public TransferenciaRealizadaConsumer(
        ITarifaRepository tarifaRepository,
        IMessageProducer messageProducer,
        IConfiguration configuration,
        ILogger<TransferenciaRealizadaConsumer> logger)
    {
        _tarifaRepository = tarifaRepository;
        _messageProducer = messageProducer;
        _configuration = configuration;
        _logger = logger;
        _valorTarifa = _configuration.GetValue<decimal>("Tarifas:ValorTransferencia", 2.00m);
    }

    public async Task Handle(IMessageContext context, TransferenciaRealizadaEvent message)
    {
        try
        {
            _logger.LogInformation("Processando transferência realizada: {IdRequisicao}", message.IdRequisicao);

            // Criar tarifa
            var tarifa = new Tarifa(
                Guid.NewGuid().ToString(),
                message.IdContaCorrente,
                _valorTarifa
            );

            // Salvar tarifa no banco
            await _tarifaRepository.SalvarAsync(tarifa);

            // Publicar evento de tarifa aplicada
            var tarifaAplicadaEvent = new TarifaAplicadaEvent
            {
                IdContaCorrente = message.IdContaCorrente,
                ValorTarifado = _valorTarifa
            };

            await _messageProducer.ProduceAsync("tarifas-realizadas", tarifaAplicadaEvent);

            _logger.LogInformation("Tarifa aplicada com sucesso: {IdContaCorrente} - {Valor}", 
                message.IdContaCorrente, _valorTarifa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar transferência realizada: {IdRequisicao}", message.IdRequisicao);
            throw;
        }
    }
}
*/
