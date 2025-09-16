using BankMore.Tarifas.Domain.Events;
using BankMore.Tarifas.Domain.Interfaces;
using BankMore.Tarifas.Infrastructure.Data;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddScoped<ITarifaRepository, TarifaRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
    using var connection = new SqliteConnection(connectionString);
    connection.Open();
    
    var sqlScripts = new[]
    {
        File.ReadAllText("../../contacorrente.sql"),
        File.ReadAllText("../../transferencia.sql"),
        File.ReadAllText("../../tarifas.sql")
    };
    
    foreach (var script in sqlScripts)
    {
        try
        {
            var command = connection.CreateCommand();
            command.CommandText = script;
            command.ExecuteNonQuery();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("already exists"))
        {
            Console.WriteLine($"Tabela já existe: {ex.Message}");
        }
    }
}


app.Run();

public interface IDbConnectionFactory
{
    SqliteConnection CreateConnection();
}

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
