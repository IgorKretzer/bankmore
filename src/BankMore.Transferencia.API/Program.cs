using BankMore.Transferencia.API.Middleware;
using BankMore.Transferencia.Domain.Handlers;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Data;
using BankMore.Transferencia.Infrastructure.Services;
using BankMore.Shared.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KafkaFlow;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

builder.Services.AddHttpClient<IContaCorrenteService, ContaCorrenteService>();

builder.Services.AddScoped<BankMore.Transferencia.Domain.Interfaces.IMessageProducer, MockMessageProducer>();


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EfetuarTransferenciaHandler).Assembly));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;
var issuer = jwtSettings["Issuer"]!;
var audience = jwtSettings["Audience"]!;

builder.Services.AddSingleton(new JwtTokenGenerator(secretKey, issuer, audience));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();

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
    private readonly string connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }
}

public class MockMessageProducer : BankMore.Transferencia.Domain.Interfaces.IMessageProducer
{
    public Task ProduceAsync<T>(string topic, T message)
    {
        Console.WriteLine($"Mock: Produzindo mensagem no tópico '{topic}': {message}");
        return Task.CompletedTask;
    }
}
