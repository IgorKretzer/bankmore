using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Commands;

public class MovimentarContaCommand : IRequest<Result>
{
    public string IdRequisicao { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public int? NumeroConta { get; set; }
    public decimal Valor { get; set; }
    public char TipoMovimento { get; set; }
}
