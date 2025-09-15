using BankMore.ContaCorrente.Domain.Entities;
using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;

namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IContaCorrenteRepository
{
    Task<ContaCorrenteEntity?> ObterPorIdAsync(string id);
    Task<ContaCorrenteEntity?> ObterPorNumeroAsync(int numero);
    Task<ContaCorrenteEntity?> ObterPorCpfAsync(string cpf);
    Task<int> ObterProximoNumeroContaAsync();
    Task SalvarAsync(ContaCorrenteEntity conta);
    Task AtualizarAsync(ContaCorrenteEntity conta);
}
