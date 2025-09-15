namespace BankMore.ContaCorrente.Domain.Entities;

public class Movimento
{
    public string IdMovimento { get; private set; }
    public string IdContaCorrente { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public char TipoMovimento { get; private set; }
    public decimal Valor { get; private set; }

    private Movimento() { }

    public Movimento(string idMovimento, string idContaCorrente, DateTime dataMovimento, char tipoMovimento, decimal valor)
    {
        IdMovimento = idMovimento;
        IdContaCorrente = idContaCorrente;
        DataMovimento = dataMovimento;
        TipoMovimento = tipoMovimento;
        Valor = valor;
    }

    public static Movimento CriarCredito(string idMovimento, string idContaCorrente, decimal valor)
    {
        return new Movimento(idMovimento, idContaCorrente, DateTime.UtcNow, 'C', valor);
    }

    public static Movimento CriarDebito(string idMovimento, string idContaCorrente, decimal valor)
    {
        return new Movimento(idMovimento, idContaCorrente, DateTime.UtcNow, 'D', valor);
    }
}
