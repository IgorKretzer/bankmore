using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.Queries;
using BankMore.Shared.Common;
using MediatR;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class ConsultarSaldoHandler : IRequestHandler<ConsultarSaldoQuery, Result<ConsultarSaldoResponse>>
{
    private readonly IContaCorrenteRepository contaRepo;
    private readonly IMovimentoRepository movimentoRepo;

    public ConsultarSaldoHandler(IContaCorrenteRepository contaRepo, IMovimentoRepository movimentoRepo)
    {
        this.contaRepo = contaRepo;
        this.movimentoRepo = movimentoRepo;
    }

    public async Task<Result<ConsultarSaldoResponse>> Handle(ConsultarSaldoQuery query, CancellationToken cancellationToken)
    {
        var contaEncontrada = await LocalizarConta(query.IdContaCorrente);
        if (contaEncontrada == null)
        {
            return Result<ConsultarSaldoResponse>.Failure("Conta não encontrada", ErrorTypes.INVALIDACCOUNT);
        }

        if (!contaEncontrada.Ativo)
        {
            return Result<ConsultarSaldoResponse>.Failure("Conta inativa", ErrorTypes.INACTIVEACCOUNT);
        }

        var historicoMovimentos = await movimentoRepo.ObterPorContaAsync(query.IdContaCorrente);
        
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
        return await contaRepo.ObterPorIdAsync(idConta);
    }

    private ValueObjects.Saldo CalcularSaldoAtual(IEnumerable<Entities.Movimento> movimentos)
    {
        return ValueObjects.Saldo.CalcularSaldo(movimentos);
    }
}
