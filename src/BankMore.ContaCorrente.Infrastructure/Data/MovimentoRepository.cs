using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.Infrastructure.Data;

public class MovimentoRepository : IMovimentoRepository
{
    private readonly string connectionString;

    public MovimentoRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task SalvarAsync(Movimento movimento)
    {
        const string sql = @"
            INSERT INTO movimento (IdMovimento, IdContaCorrente, DataMovimento, TipoMovimento, Valor)
            VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";

        using var connection = new SqliteConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            IdMovimento = movimento.IdMovimento,
            IdContaCorrente = movimento.IdContaCorrente,
            DataMovimento = movimento.DataMovimento.ToString("dd/MM/yyyy HH:mm:ss"),
            TipoMovimento = movimento.TipoMovimento.ToString(),
            Valor = movimento.Valor
        });
    }

    public async Task<IEnumerable<Movimento>> ObterPorContaAsync(string idContaCorrente)
    {
        const string sql = @"
            SELECT IdMovimento, IdContaCorrente, DataMovimento, TipoMovimento, Valor
            FROM movimento 
            WHERE IdContaCorrente = @IdContaCorrente
            ORDER BY DataMovimento";

        using var connection = new SqliteConnection(connectionString);
        var results = await connection.QueryAsync<MovimentoDto>(sql, new { IdContaCorrente = idContaCorrente });
        
        return results.Select(dto => dto.ToEntity());
    }

    public async Task<bool> ExisteMovimentoAsync(string idMovimento)
    {
        const string sql = "SELECT COUNT(1) FROM movimento WHERE IdMovimento = @IdMovimento";

        using var connection = new SqliteConnection(connectionString);
        var count = await connection.QuerySingleAsync<int>(sql, new { IdMovimento = idMovimento });
        
        return count > 0;
    }
}

internal class MovimentoDto
{
    public string IdMovimento { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public string DataMovimento { get; set; } = string.Empty;
    public string TipoMovimento { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    public Movimento ToEntity()
    {
        return new Movimento(
            IdMovimento,
            IdContaCorrente,
            DateTime.ParseExact(DataMovimento, "dd/MM/yyyy HH:mm:ss", null),
            TipoMovimento[0],
            Valor
        );
    }
}
