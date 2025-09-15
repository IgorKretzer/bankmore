using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Commands;

public class CadastrarContaCommand : IRequest<Result<CadastrarContaResponse>>
{
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class CadastrarContaResponse
{
    public string IdContaCorrente { get; set; } = string.Empty;
    public int NumeroConta { get; set; }
}
