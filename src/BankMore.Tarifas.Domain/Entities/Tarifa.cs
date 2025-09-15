namespace BankMore.Tarifas.Domain.Entities;

public class Tarifa
{
    public string IdTarifa { get; private set; }
    public string IdContaCorrente { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public decimal Valor { get; private set; }

    private Tarifa() { }

    public Tarifa(string idTarifa, string idContaCorrente, decimal valor)
    {
        IdTarifa = idTarifa;
        IdContaCorrente = idContaCorrente;
        DataMovimento = DateTime.UtcNow;
        Valor = valor;
    }
}
