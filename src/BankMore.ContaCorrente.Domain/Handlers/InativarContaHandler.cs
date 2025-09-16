using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class InativarContaHandler : IRequestHandler<InativarContaCommand, Result>
{
    private readonly IContaCorrenteRepository contaRepository;

    public InativarContaHandler(IContaCorrenteRepository contaRepository)
    {
        this.contaRepository = contaRepository;
    }

    public async Task<Result> Handle(InativarContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await contaRepository.ObterPorIdAsync(request.IdContaCorrente);
        
        if (conta == null)
        {
            return Result.Failure("Conta não encontrada", ErrorTypes.INVALIDACCOUNT);
        }

        if (!conta.VerificarSenha(request.Senha, conta.Senha, conta.Salt))
        {
            return Result.Failure("Senha inválida", ErrorTypes.USERUNAUTHORIZED);
        }

        if (!conta.Ativo)
        {
            return Result.Failure("Conta já está inativa", ErrorTypes.INACTIVEACCOUNT);
        }

        conta.Inativar();
        await contaRepository.AtualizarAsync(conta);

        return Result.Success();
    }
}
