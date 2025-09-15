using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Identificacao { get; set; } = string.Empty; // CPF ou número da conta
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public int NumeroConta { get; set; }
}
