using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Commands;

public class InativarContaCommand : IRequest<Result>
{
    public string IdContaCorrente { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
