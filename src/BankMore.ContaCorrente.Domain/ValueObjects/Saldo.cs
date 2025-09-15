using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.ValueObjects;

public class Saldo
{
    public decimal Valor { get; private set; }
    public DateTime DataConsulta { get; private set; }

    public Saldo(decimal valor)
    {
        Valor = valor;
        DataConsulta = DateTime.UtcNow;
    }

    public static Saldo CalcularSaldo(IEnumerable<Movimento> movimentos)
    {
        var creditos = movimentos.Where(m => m.TipoMovimento == 'C').Sum(m => m.Valor);
        var debitos = movimentos.Where(m => m.TipoMovimento == 'D').Sum(m => m.Valor);
        
        return new Saldo(creditos - debitos);
    }
}
