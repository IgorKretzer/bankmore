using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;
using Microsoft.Extensions.Configuration;

namespace BankMore.Transferencia.Infrastructure.Data;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly string _connectionString;

    public TransferenciaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<TransferenciaEntity?> ObterPorIdAsync(string id)
    {
        const string sql = @"
            SELECT IdTransferencia, IdContaCorrenteOrigem, IdContaCorrenteDestino, Valor, DataTransferencia
            FROM transferencia 
            WHERE IdTransferencia = @Id";

        using var connection = new SqliteConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<TransferenciaDto>(sql, new { Id = id });
        
        return result?.ToEntity();
    }

    public async Task SalvarAsync(TransferenciaEntity transferencia)
    {
        const string sql = @"
            INSERT INTO transferencia (IdTransferencia, IdContaCorrente_Origem, IdContaCorrente_Destino, DataMovimento, Valor)
            VALUES (@IdTransferencia, @IdContaCorrenteOrigem, @IdContaCorrenteDestino, @DataTransferencia, @Valor)";

        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            IdTransferencia = transferencia.IdTransferencia,
            IdContaCorrenteOrigem = transferencia.IdContaCorrenteOrigem,
            IdContaCorrenteDestino = transferencia.IdContaCorrenteDestino,
            DataTransferencia = transferencia.DataTransferencia.ToString("dd/MM/yyyy HH:mm:ss"),
            Valor = transferencia.Valor
        });
    }
}

public class TransferenciaDto
{
    public string IdTransferencia { get; set; } = string.Empty;
    public string IdContaCorrenteOrigem { get; set; } = string.Empty;
    public string IdContaCorrenteDestino { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataTransferencia { get; set; }

    public TransferenciaEntity ToEntity()
    {
        return new TransferenciaEntity(
            IdTransferencia,
            IdContaCorrenteOrigem,
            IdContaCorrenteDestino,
            Valor);
    }
}
