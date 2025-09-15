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
        // Primeiro, validar os dados básicos - aprendi que é melhor fazer isso antes de qualquer operação de banco
        var validacaoResult = await ValidarDadosBasicos(request);
        if (validacaoResult.IsFailure)
            return validacaoResult;

        // Verificar se CPF já existe - isso é crítico para evitar duplicatas
        var contaExistente = await _contaRepository.ObterPorCpfAsync(request.Cpf);
        if (contaExistente != null)
        {
            return Result<CadastrarContaResponse>.Failure("CPF já cadastrado", ErrorTypes.INVALID_DOCUMENT);
        }

        // Gerar identificadores únicos
        var idConta = GerarIdConta();
        var numeroConta = await ObterProximoNumeroConta();

        // Processar senha com hash e salt - importante para segurança
        var credenciais = ProcessarSenha(request.Senha);

        // Criar a entidade conta
        var conta = CriarNovaConta(idConta, numeroConta, request.Nome, credenciais.hash, credenciais.salt);

        // Persistir no banco de dados
        await _contaRepository.SalvarAsync(conta);

        // Retornar resposta de sucesso
        return Result<CadastrarContaResponse>.Success(new CadastrarContaResponse
        {
            IdContaCorrente = idConta,
            NumeroConta = numeroConta
        });
    }

    private Task<Result<CadastrarContaResponse>> ValidarDadosBasicos(CadastrarContaCommand request)
    {
        // Validar CPF - usando a validação que já existe
        if (!ValidationHelper.IsValidCpf(request.Cpf))
        {
            return Task.FromResult(Result<CadastrarContaResponse>.Failure("CPF inválido", ErrorTypes.INVALID_DOCUMENT));
        }

        // Validar senha - mínimo de 6 caracteres por enquanto
        if (!ValidationHelper.IsValidPassword(request.Senha))
        {
            return Task.FromResult(Result<CadastrarContaResponse>.Failure("Senha deve ter pelo menos 6 caracteres", ErrorTypes.INVALID_VALUE));
        }

        // TODO: Adicionar validação de nome (não pode ser vazio, caracteres especiais, etc.)
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Task.FromResult(Result<CadastrarContaResponse>.Failure("Nome é obrigatório", ErrorTypes.INVALID_VALUE));
        }

        return Task.FromResult(Result<CadastrarContaResponse>.Success(null!)); // Só para indicar que passou na validação
    }

    private string GerarIdConta()
    {
        // Usando Guid para garantir unicidade
        return Guid.NewGuid().ToString();
    }

    private async Task<int> ObterProximoNumeroConta()
    {
        // Delegando para o repositório - ele que sabe como gerar o próximo número
        return await _contaRepository.ObterProximoNumeroContaAsync();
    }

    private (string hash, string salt) ProcessarSenha(string senha)
    {
        // Usando o helper de hash que já existe
        return PasswordHasher.HashPassword(senha);
    }

    private Entities.ContaCorrente CriarNovaConta(string idConta, int numeroConta, string nome, string hash, string salt)
    {
        // Criando a entidade com os dados processados
        return new Entities.ContaCorrente(idConta, numeroConta, nome, hash, salt);
    }
}
