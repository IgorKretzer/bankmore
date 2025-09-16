using BankMore.ContaCorrente.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace BankMore.ContaCorrente.Infrastructure.Data;

public class IdempotenciaRepository : IIdempotenciaRepository
{
    private readonly string _connectionString;
    private readonly IMemoryCache _cache;

    public IdempotenciaRepository(IConfiguration configuration, IMemoryCache cache)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _cache = cache;
    }

    public async Task<string?> ObterResultadoAsync(string chaveIdempotencia)
    {
        if (_cache.TryGetValue(chaveIdempotencia, out string? cachedResult))
        {
            return cachedResult;
        }

        const string sql = "SELECT Resultado FROM idempotencia WHERE Chave_Idempotencia = @Chave";

        using var connection = new SqliteConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Chave = chaveIdempotencia });
        
        if (!string.IsNullOrEmpty(result))
        {
            _cache.Set(chaveIdempotencia, result, TimeSpan.FromHours(1));
        }

        return result;
    }

    public async Task SalvarAsync(string chaveIdempotencia, string requisicao, string resultado)
    {
        const string sql = @"
            INSERT OR REPLACE INTO idempotencia (Chave_Idempotencia, Requisicao, Resultado)
            VALUES (@Chave, @Requisicao, @Resultado)";

        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            Chave = chaveIdempotencia,
            Requisicao = requisicao,
            Resultado = resultado
        });

        _cache.Set(chaveIdempotencia, resultado, TimeSpan.FromHours(1));
    }
}
