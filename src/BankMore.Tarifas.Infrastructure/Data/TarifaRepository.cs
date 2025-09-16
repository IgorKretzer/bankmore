using BankMore.Tarifas.Domain.Entities;
using BankMore.Tarifas.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankMore.Tarifas.Infrastructure.Data;

public class TarifaRepository : ITarifaRepository
{
    private readonly string connectionString;

    public TarifaRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task SalvarAsync(Tarifa tarifa)
    {
        const string sql = @"
            INSERT INTO tarifa (IdTarifa, IdContaCorrente, DataMovimento, Valor)
            VALUES (@IdTarifa, @IdContaCorrente, @DataMovimento, @Valor)";

        using var connection = new SqliteConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            IdTarifa = tarifa.IdTarifa,
            IdContaCorrente = tarifa.IdContaCorrente,
            DataMovimento = tarifa.DataMovimento.ToString("dd/MM/yyyy HH:mm:ss"),
            Valor = tarifa.Valor
        });
    }
}
