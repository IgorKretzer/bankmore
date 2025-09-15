using BankMore.Transferencia.Domain.Entities;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task<TransferenciaEntity?> ObterPorIdAsync(string id);
    Task SalvarAsync(TransferenciaEntity transferencia);
}
