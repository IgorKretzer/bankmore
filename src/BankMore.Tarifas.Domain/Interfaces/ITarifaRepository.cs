using BankMore.Tarifas.Domain.Entities;

namespace BankMore.Tarifas.Domain.Interfaces;

public interface ITarifaRepository
{
    Task SalvarAsync(Tarifa tarifa);
}
