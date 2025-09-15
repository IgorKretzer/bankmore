namespace BankMore.Tarifas.Domain.Events;

public class TarifaAplicadaEvent
{
    public string IdContaCorrente { get; set; } = string.Empty;
    public decimal ValorTarifado { get; set; }
    public DateTime DataEvento { get; set; } = DateTime.UtcNow;
}
