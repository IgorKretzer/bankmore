using BankMore.Shared.Common;
using MediatR;

namespace BankMore.Transferencia.Domain.Commands;

public class EfetuarTransferenciaCommand : IRequest<Result>
{
    public string IdRequisicao { get; set; } = string.Empty;
    public string IdContaCorrenteOrigem { get; set; } = string.Empty;
    public int NumeroContaDestino { get; set; }
    public decimal Valor { get; set; }
    public string Token { get; set; } = string.Empty;
}
