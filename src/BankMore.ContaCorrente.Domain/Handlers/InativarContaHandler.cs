using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class InativarContaHandler : IRequestHandler<InativarContaCommand, Result>
{
    private readonly IContaCorrenteRepository _contaRepository;

    public InativarContaHandler(IContaCorrenteRepository contaRepository)
    {
        _contaRepository = contaRepository;
    }

    public async Task<Result> Handle(InativarContaCommand request, CancellationToken cancellationToken)
    {
        // Buscar conta
        var conta = await _contaRepository.ObterPorIdAsync(request.IdContaCorrente);
        
        if (conta == null)
        {
            return Result.Failure("Conta não encontrada", ErrorTypes.INVALID_ACCOUNT);
        }

        // Verificar senha
        if (!conta.VerificarSenha(request.Senha, conta.Senha, conta.Salt))
        {
            return Result.Failure("Senha inválida", ErrorTypes.USER_UNAUTHORIZED);
        }

        // Verificar se conta está ativa
        if (!conta.Ativo)
        {
            return Result.Failure("Conta já está inativa", ErrorTypes.INACTIVE_ACCOUNT);
        }

        // Inativar conta
        conta.Inativar();
        await _contaRepository.AtualizarAsync(conta);

        return Result.Success();
    }
}
