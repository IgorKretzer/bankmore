namespace BankMore.Transferencia.Domain.Interfaces;

public interface IMessageProducer
{
    Task ProduceAsync<T>(string topic, T message);
}
