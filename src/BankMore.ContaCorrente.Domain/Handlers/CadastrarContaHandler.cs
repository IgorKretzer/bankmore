using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using BankMore.Shared.Security;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class CadastrarContaHandler : IRequestHandler<CadastrarContaCommand, Result<CadastrarContaResponse>>
{
    private readonly IContaCorrenteRepository _contaRepository;

    public CadastrarContaHandler(IContaCorrenteRepository contaRepository)
    {
        _contaRepository = contaRepository;
    }

    public async Task<Result<CadastrarContaResponse>> Handle(CadastrarContaCommand request, CancellationToken cancellationToken)
    {
        var validacaoResult = await ValidarDadosBasicos(request);
        if (validacaoResult.IsFailure)
            return validacaoResult;

        var contaExistente = await _contaRepository.ObterPorCpfAsync(request.Cpf);
        if (contaExistente != null)
        {
            return Result<CadastrarContaResponse>.Failure("CPF já cadastrado", ErrorTypes.INVALID_DOCUMENT);
        }

        var idConta = GerarIdConta();
        var numeroConta = await ObterProximoNumeroConta();

        var credenciais = ProcessarSenha(request.Senha);

        var conta = CriarNovaConta(idConta, numeroConta, request.Nome, credenciais.hash, credenciais.salt);

        await _contaRepository.SalvarAsync(conta);

        return Result<CadastrarContaResponse>.Success(new CadastrarContaResponse
        {
            IdContaCorrente = idConta,
            NumeroConta = numeroConta
        });
    }

    private Task<Result<CadastrarContaResponse>> ValidarDadosBasicos(CadastrarContaCommand request)
    {
        if (!ValidationHelper.IsValidCpf(request.Cpf))
        {
            return Task.FromResult(Result<CadastrarContaResponse>.Failure("CPF inválido", ErrorTypes.INVALID_DOCUMENT));
        }

        if (!ValidationHelper.IsValidPassword(request.Senha))
        {
            return Task.FromResult(Result<CadastrarContaResponse>.Failure("Senha deve ter pelo menos 6 caracteres", ErrorTypes.INVALID_VALUE));
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Task.FromResult(Result<CadastrarContaResponse>.Failure("Nome é obrigatório", ErrorTypes.INVALID_VALUE));
        }

        return Task.FromResult(Result<CadastrarContaResponse>.Success(null!)); // Só para indicar que passou na validação
    }

    private string GerarIdConta()
    {
        return Guid.NewGuid().ToString();
    }

    private async Task<int> ObterProximoNumeroConta()
    {
        return await _contaRepository.ObterProximoNumeroContaAsync();
    }

    private (string hash, string salt) ProcessarSenha(string senha)
    {
        return PasswordHasher.HashPassword(senha);
    }

    private Entities.ContaCorrente CriarNovaConta(string idConta, int numeroConta, string nome, string hash, string salt)
    {
        return new Entities.ContaCorrente(idConta, numeroConta, nome, hash, salt);
    }
}
