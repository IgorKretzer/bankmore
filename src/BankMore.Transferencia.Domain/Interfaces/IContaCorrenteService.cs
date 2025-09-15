using BankMore.Shared.Common;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface IContaCorrenteService
{
    Task<Result> RealizarDebitoAsync(string idRequisicao, string idContaCorrente, decimal valor, string token);
    Task<Result> RealizarCreditoAsync(string idRequisicao, int numeroConta, decimal valor, string token);
}
