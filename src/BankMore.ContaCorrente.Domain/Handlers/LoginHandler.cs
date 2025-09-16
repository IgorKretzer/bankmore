using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using BankMore.Shared.Security;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly JwtTokenGenerator _jwtGenerator;

    public LoginHandler(IContaCorrenteRepository contaRepository, JwtTokenGenerator jwtGenerator)
    {
        _contaRepository = contaRepository;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var conta = await LocalizarConta(request.Identificacao);
        
        if (conta == null)
        {
            return Result<LoginResponse>.Failure("Conta não encontrada", ErrorTypes.INVALID_ACCOUNT);
        }

        if (!conta.Ativo)
        {
            return Result<LoginResponse>.Failure("Conta inativa", ErrorTypes.INACTIVE_ACCOUNT);
        }

        var credenciaisValidas = ValidarCredenciais(conta, request.Senha);
        if (!credenciaisValidas)
        {
            return Result<LoginResponse>.Failure("Senha inválida", ErrorTypes.USER_UNAUTHORIZED);
        }

        var token = GerarTokenJwt(conta);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            IdContaCorrente = conta.IdContaCorrente,
            NumeroConta = conta.Numero
        });
    }

    private async Task<Entities.ContaCorrente?> LocalizarConta(string identificacao)
    {
        if (EhNumeroConta(identificacao))
        {
            return await BuscarPorNumero(identificacao);
        }

        return await BuscarPorCpf(identificacao);
    }

    private bool EhNumeroConta(string identificacao)
    {
        return int.TryParse(identificacao, out _);
    }

    private async Task<Entities.ContaCorrente?> BuscarPorNumero(string identificacao)
    {
        var numeroConta = int.Parse(identificacao);
        return await _contaRepository.ObterPorNumeroAsync(numeroConta);
    }

    private async Task<Entities.ContaCorrente?> BuscarPorCpf(string identificacao)
    {
        return await _contaRepository.ObterPorCpfAsync(identificacao);
    }

    private bool ValidarCredenciais(Entities.ContaCorrente conta, string senha)
    {
        return conta.VerificarSenha(senha, conta.Senha, conta.Salt);
    }

    private string GerarTokenJwt(Entities.ContaCorrente conta)
    {
        return _jwtGenerator.GenerateToken(conta.IdContaCorrente, conta.Numero.ToString());
    }
}
