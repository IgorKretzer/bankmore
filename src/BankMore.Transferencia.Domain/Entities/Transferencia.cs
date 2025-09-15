namespace BankMore.Transferencia.Domain.Entities;

public class Transferencia
{
    public string IdTransferencia { get; private set; }
    public string IdContaCorrenteOrigem { get; private set; }
    public string IdContaCorrenteDestino { get; private set; }
    public DateTime DataTransferencia { get; private set; }
    public decimal Valor { get; private set; }

    private Transferencia() { }

    public Transferencia(string idTransferencia, string idContaCorrenteOrigem, string idContaCorrenteDestino, decimal valor)
    {
        IdTransferencia = idTransferencia;
        IdContaCorrenteOrigem = idContaCorrenteOrigem;
        IdContaCorrenteDestino = idContaCorrenteDestino;
        DataTransferencia = DateTime.UtcNow;
        Valor = valor;
    }
}
