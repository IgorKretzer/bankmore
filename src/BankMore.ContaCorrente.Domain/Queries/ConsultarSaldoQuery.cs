using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Queries;

public class ConsultarSaldoQuery : IRequest<Result<ConsultarSaldoResponse>>
{
    public string IdContaCorrente { get; set; } = string.Empty;
}

public class ConsultarSaldoResponse
{
    public int NumeroConta { get; set; }
    public string NomeTitular { get; set; } = string.Empty;
    public DateTime DataConsulta { get; set; }
    public decimal Saldo { get; set; }
}
