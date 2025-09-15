namespace BankMore.Transferencia.Domain.Events;

public class TransferenciaRealizadaEvent
{
    public string IdRequisicao { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; } = DateTime.UtcNow;
}
