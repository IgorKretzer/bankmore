using BankMore.Transferencia.Domain.Commands;
using BankMore.Transferencia.Domain.Handlers;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Domain.Events;
using BankMore.Shared.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankMore.Transferencia.Tests.Handlers;

public class EfetuarTransferenciaHandlerTests
{
    private readonly Mock<ITransferenciaRepository> transferenciaRepositoryMock;
    private readonly Mock<IContaCorrenteService> contaCorrenteServiceMock;
    private readonly Mock<IMessageProducer> messageProducerMock;
    private readonly EfetuarTransferenciaHandler handler;

    public EfetuarTransferenciaHandlerTests()
    {
        transferenciaRepositoryMock = new Mock<ITransferenciaRepository>();
        contaCorrenteServiceMock = new Mock<IContaCorrenteService>();
        messageProducerMock = new Mock<IMessageProducer>();
        handler = new EfetuarTransferenciaHandler(
            transferenciaRepositoryMock.Object,
            contaCorrenteServiceMock.Object,
            messageProducerMock.Object);
    }

    [Fact]
    public async Task Handle_ComValorValido_DeveRetornarSucesso()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand
        {
            IdRequisicao = "trans-001",
            IdContaCorrenteOrigem = "conta-origem-123",
            NumeroContaDestino = 2,
            Valor = 100.50m,
            Token = "jwt-token-123"
        };

        contaCorrenteServiceMock.Setup(x => x.RealizarDebitoAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        contaCorrenteServiceMock.Setup(x => x.RealizarCreditoAsync(
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        transferenciaRepositoryMock.Setup(x => x.SalvarAsync(It.IsAny<Domain.Entities.Transferencia>()))
            .Returns(Task.CompletedTask);

        messageProducerMock.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        contaCorrenteServiceMock.Verify(x => x.RealizarDebitoAsync(
            "trans-001_debito", 
            "conta-origem-123", 
            100.50m, 
            "jwt-token-123"), Times.Once);
        contaCorrenteServiceMock.Verify(x => x.RealizarCreditoAsync(
            "trans-001_credito", 
            2, 
            100.50m, 
            "jwt-token-123"), Times.Once);
        transferenciaRepositoryMock.Verify(x => x.SalvarAsync(It.IsAny<Domain.Entities.Transferencia>()), Times.Once);
        messageProducerMock.Verify(x => x.ProduceAsync("transferencias-realizadas", It.IsAny<TransferenciaRealizadaEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComValorInvalido_DeveRetornarFalha()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand
        {
            IdRequisicao = "trans-002",
            IdContaCorrenteOrigem = "conta-origem-123",
            NumeroContaDestino = 2,
            Valor = -50.00m, // Valor negativo
            Token = "jwt-token-123"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Valor deve ser positivo");
        result.ErrorType.Should().Be(ErrorTypes.INVALID_VALUE);
    }

    [Fact]
    public async Task Handle_ComFalhaNoDebito_DeveRetornarFalha()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand
        {
            IdRequisicao = "trans-003",
            IdContaCorrenteOrigem = "conta-origem-123",
            NumeroContaDestino = 2,
            Valor = 100.00m,
            Token = "jwt-token-123"
        };

        contaCorrenteServiceMock.Setup(x => x.RealizarDebitoAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()))
            .ReturnsAsync(Result.Failure("Saldo insuficiente", ErrorTypes.INSUFFICIENT_FUNDS));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Falha no débito: Saldo insuficiente");
        result.ErrorType.Should().Be(ErrorTypes.TRANSFER_FAILED);
    }

    [Fact]
    public async Task Handle_ComFalhaNoCredito_DeveRetornarFalha()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand
        {
            IdRequisicao = "trans-004",
            IdContaCorrenteOrigem = "conta-origem-123",
            NumeroContaDestino = 2,
            Valor = 100.00m,
            Token = "jwt-token-123"
        };

        contaCorrenteServiceMock.Setup(x => x.RealizarDebitoAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        contaCorrenteServiceMock.Setup(x => x.RealizarCreditoAsync(
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()))
            .ReturnsAsync(Result.Failure("Conta destino inativa", ErrorTypes.INACTIVE_ACCOUNT));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Falha no crédito: Conta destino inativa");
        result.ErrorType.Should().Be(ErrorTypes.TRANSFER_FAILED);
    }

    [Fact]
    public async Task Handle_ComExcecao_DeveRetornarFalha()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand
        {
            IdRequisicao = "trans-005",
            IdContaCorrenteOrigem = "conta-origem-123",
            NumeroContaDestino = 2,
            Valor = 100.00m,
            Token = "jwt-token-123"
        };

        contaCorrenteServiceMock.Setup(x => x.RealizarDebitoAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()))
            .ThrowsAsync(new Exception("Erro de conexão"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Erro interno: Erro de conexão");
        result.ErrorType.Should().Be(ErrorTypes.INTERNAL_ERROR);
    }
}
