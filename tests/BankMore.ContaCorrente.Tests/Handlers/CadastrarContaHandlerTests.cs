using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Handlers;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.Shared.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Handlers;

public class CadastrarContaHandlerTests
{
    private readonly Mock<IContaCorrenteRepository> _repositoryMock;
    private readonly CadastrarContaHandler _handler;

    public CadastrarContaHandlerTests()
    {
        _repositoryMock = new Mock<IContaCorrenteRepository>();
        _handler = new CadastrarContaHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComCpfValido_DeveRetornarSucesso()
    {
        // Arrange
        var command = new CadastrarContaCommand
        {
            Cpf = "11144477735", // CPF válido
            Nome = "João Silva",
            Senha = "123456"
        };

        _repositoryMock.Setup(x => x.ObterPorCpfAsync(It.IsAny<string>()))
            .ReturnsAsync((ContaCorrente.Domain.Entities.ContaCorrente?)null);
        _repositoryMock.Setup(x => x.ObterProximoNumeroContaAsync())
            .ReturnsAsync(1001);
        _repositoryMock.Setup(x => x.SalvarAsync(It.IsAny<ContaCorrente.Domain.Entities.ContaCorrente>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NumeroConta.Should().Be(1001);
        _repositoryMock.Verify(x => x.SalvarAsync(It.IsAny<ContaCorrente.Domain.Entities.ContaCorrente>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComCpfInvalido_DeveRetornarFalha()
    {
        // Arrange
        var command = new CadastrarContaCommand
        {
            Cpf = "123",
            Nome = "João Silva",
            Senha = "123456"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorTypes.INVALID_DOCUMENT);
    }

    [Fact]
    public async Task Handle_ComSenhaInvalida_DeveRetornarFalha()
    {
        // Arrange
        var command = new CadastrarContaCommand
        {
            Cpf = "11144477735", // CPF válido
            Nome = "João Silva",
            Senha = "123" // Senha inválida (menos de 6 caracteres)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorTypes.INVALID_VALUE);
    }
}
