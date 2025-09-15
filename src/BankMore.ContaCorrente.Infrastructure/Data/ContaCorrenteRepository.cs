using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using ContaCorrenteEntity = BankMore.ContaCorrente.Domain.Entities.ContaCorrente;

namespace BankMore.ContaCorrente.Infrastructure.Data;

public class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly string _connectionString;
    private readonly IMemoryCache _cache;

    public ContaCorrenteRepository(IConfiguration configuration, IMemoryCache cache)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _cache = cache;
    }

    public async Task<ContaCorrenteEntity?> ObterPorIdAsync(string id)
    {
        const string sql = @"
            SELECT IdContaCorrente, Numero, Nome, Ativo, Senha, Salt 
            FROM contacorrente 
            WHERE IdContaCorrente = @Id";

        using var connection = new SqliteConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<ContaCorrenteDto>(sql, new { Id = id });
        
        return result?.ToEntity();
    }

    public async Task<ContaCorrenteEntity?> ObterPorNumeroAsync(int numero)
    {
        const string sql = @"
            SELECT IdContaCorrente, Numero, Nome, Ativo, Senha, Salt 
            FROM contacorrente 
            WHERE Numero = @Numero";

        using var connection = new SqliteConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<ContaCorrenteDto>(sql, new { Numero = numero });
        
        return result?.ToEntity();
    }

    public async Task<ContaCorrenteEntity?> ObterPorCpfAsync(string cpf)
    {
        // IMPLEMENTAÇÃO TEMPORÁRIA: usando CPF como parte do nome
        // Decisão: Para simplificar o modelo de dados, estou armazenando CPF no campo Nome
        // Em produção real, teria uma tabela separada para dados pessoais (CPF, RG, etc.)
        // e uma foreign key para a tabela de contas
        // TODO: Refatorar para ter tabela de dados pessoais separada
        
        const string sql = @"
            SELECT IdContaCorrente, Numero, Nome, Ativo, Senha, Salt 
            FROM contacorrente 
            WHERE Nome LIKE @Cpf";

        using var connection = new SqliteConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<ContaCorrenteDto>(sql, new { Cpf = $"%{cpf}%" });
        
        return result?.ToEntity();
    }

    public async Task<int> ObterProximoNumeroContaAsync()
    {
        // Geração de número sequencial - pode ter problemas de concorrência
        // Em produção, usaria uma tabela de sequências ou UUID
        // Para este teste, funciona porque não há alta concorrência
        const string sql = "SELECT COALESCE(MAX(Numero), 0) + 1 FROM contacorrente";

        using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(sql);
    }

    public async Task SalvarAsync(ContaCorrenteEntity conta)
    {
        const string sql = @"
            INSERT INTO contacorrente (IdContaCorrente, Numero, Nome, Ativo, Senha, Salt)
            VALUES (@IdContaCorrente, @Numero, @Nome, @Ativo, @Senha, @Salt)";

        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            IdContaCorrente = conta.IdContaCorrente,
            Numero = conta.Numero,
            Nome = conta.Nome,
            Ativo = conta.Ativo ? 1 : 0,
            Senha = conta.Senha,
            Salt = conta.Salt
        });
    }

    public async Task AtualizarAsync(ContaCorrenteEntity conta)
    {
        const string sql = @"
            UPDATE contacorrente 
            SET Ativo = @Ativo
            WHERE IdContaCorrente = @IdContaCorrente";

        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            IdContaCorrente = conta.IdContaCorrente,
            Ativo = conta.Ativo ? 1 : 0
        });
    }
}

internal class ContaCorrenteDto
{
    public string IdContaCorrente { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ativo { get; set; }
    public string Senha { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    public ContaCorrenteEntity ToEntity()
    {
        var conta = new ContaCorrenteEntity(IdContaCorrente, Numero, Nome, Senha, Salt);
        if (Ativo == 0)
        {
            conta.Inativar();
        }
        return conta;
    }
}
