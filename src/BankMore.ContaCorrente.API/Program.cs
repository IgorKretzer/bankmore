using BankMore.ContaCorrente.API.Middleware;
using BankMore.ContaCorrente.Domain.Handlers;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using BankMore.Shared.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - usando SQLite para simplificar o setup, mas em produção seria PostgreSQL
// Decidi por SQLite porque é mais fácil de configurar e não precisa de servidor separado
builder.Services.AddScoped<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

// Repositories - registrando as implementações concretas
// Uso Scoped para garantir que cada request tenha sua própria instância
builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// MediatR - configurando para usar os handlers do assembly atual
// Isso permite usar CQRS de forma mais limpa
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CadastrarContaHandler).Assembly));

// JWT - configuração de autenticação
// Decidi usar JWT porque é stateless e funciona bem com microsserviços
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;
var issuer = jwtSettings["Issuer"]!;
var audience = jwtSettings["Audience"]!;

// Singleton porque o gerador de token não tem estado
builder.Services.AddSingleton(new JwtTokenGenerator(secretKey, issuer, audience));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configuração rigorosa de validação - importante para segurança bancária
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Sem tolerância para expiração
        };
    });

builder.Services.AddAuthorization();

// Memory Cache - para idempotência e performance
// Em produção, consideraria usar Redis para cache distribuído
builder.Services.AddMemoryCache();

// CORS - configurado para permitir todas as origens (apenas para desenvolvimento)
// Em produção, seria mais restritivo
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

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
