using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.Queries;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class ConsultarSaldoHandler : IRequestHandler<ConsultarSaldoQuery, Result<ConsultarSaldoResponse>>
{
    private readonly IContaCorrenteRepository _contaRepo;
    private readonly IMovimentoRepository _movimentoRepo;

    public ConsultarSaldoHandler(IContaCorrenteRepository contaRepo, IMovimentoRepository movimentoRepo)
    {
        _contaRepo = contaRepo;
        _movimentoRepo = movimentoRepo;
    }

    public async Task<Result<ConsultarSaldoResponse>> Handle(ConsultarSaldoQuery query, CancellationToken cancellationToken)
    {
        var contaEncontrada = await LocalizarConta(query.IdContaCorrente);
        if (contaEncontrada == null)
        {
            return Result<ConsultarSaldoResponse>.Failure("Conta não encontrada", ErrorTypes.INVALID_ACCOUNT);
        }

        if (!contaEncontrada.Ativo)
        {
            return Result<ConsultarSaldoResponse>.Failure("Conta inativa", ErrorTypes.INACTIVE_ACCOUNT);
        }

        var historicoMovimentos = await _movimentoRepo.ObterPorContaAsync(query.IdContaCorrente);
        
        var saldoAtual = CalcularSaldoAtual(historicoMovimentos);

        return Result<ConsultarSaldoResponse>.Success(new ConsultarSaldoResponse
        {
            NumeroConta = contaEncontrada.Numero,
            NomeTitular = contaEncontrada.Nome,
            DataConsulta = saldoAtual.DataConsulta,
            Saldo = saldoAtual.Valor
        });
    }

    private async Task<Entities.ContaCorrente?> LocalizarConta(string idConta)
    {
        return await _contaRepo.ObterPorIdAsync(idConta);
    }

    private ValueObjects.Saldo CalcularSaldoAtual(IEnumerable<Entities.Movimento> movimentos)
    {
        return ValueObjects.Saldo.CalcularSaldo(movimentos);
    }
}
