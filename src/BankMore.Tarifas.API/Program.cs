using BankMore.Tarifas.Domain.Events;
using BankMore.Tarifas.Domain.Interfaces;
using BankMore.Tarifas.Infrastructure.Data;
// using BankMore.Tarifas.Infrastructure.Kafka;
// using KafkaFlow;
// using KafkaFlow.Microsoft.DependencyInjection;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddScoped<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

// Repositories
builder.Services.AddScoped<ITarifaRepository, TarifaRepository>();

// Kafka (comentado por enquanto)
// builder.Services.AddKafka(kafka => kafka
//     .UseConsoleLog()
//     .AddCluster(cluster => cluster
//         .WithBrokers(new[] { "localhost:9092" })
//         .AddConsumer(consumer => consumer
//             .Topic("transferencias-realizadas")
//             .WithGroupId("tarifas-group")
//             .WithBufferSize(100)
//             .WithWorkersCount(10)
//             .AddMiddlewares(middlewares => middlewares
//                 .AddDeserializer<JsonDeserializer>()
//                 .AddTypedHandlers(handlers => handlers
//                     .AddHandler<TransferenciaRealizadaConsumer>()
//                 )
//             )
//         )
//         .AddProducer(producer => producer
//             .DefaultTopic("tarifas-realizadas")
//             .AddMiddlewares(middlewares => middlewares
//                 .AddSerializer<JsonSerializer>()
//             )
//         )
//     )
// );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
    using var connection = new SqliteConnection(connectionString);
    connection.Open();
    
    // Execute SQL scripts
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
            // Tabela já existe, continuar
            Console.WriteLine($"Tabela já existe: {ex.Message}");
        }
    }
}

// Start Kafka (comentado por enquanto)
// var bus = app.Services.CreateKafkaBus();
// await bus.StartAsync();

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
