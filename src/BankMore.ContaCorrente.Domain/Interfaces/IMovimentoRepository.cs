using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IMovimentoRepository
{
    Task SalvarAsync(Movimento movimento);
    Task<IEnumerable<Movimento>> ObterPorContaAsync(string idContaCorrente);
    Task<bool> ExisteMovimentoAsync(string idMovimento);
}
