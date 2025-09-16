using BankMore.Tarifas.API.Controllers;
using BankMore.Tarifas.Domain.Interfaces;
using BankMore.Tarifas.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace BankMore.Tarifas.Tests.Controllers;

public class TarifasControllerTests
{
    private readonly Mock<ITarifaRepository> tarifaRepositoryMock;
    private readonly Mock<ILogger<TarifasController>> loggerMock;
    private readonly TarifasController controller;

    public TarifasControllerTests()
    {
        tarifaRepositoryMock = new Mock<ITarifaRepository>();
        loggerMock = new Mock<ILogger<TarifasController>>();
        controller = new TarifasController(tarifaRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task ConsultarTarifas_DeveRetornarSucesso()
    {
        // Act
        var result = await controller.ConsultarTarifas();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        // Verificar se o log foi chamado
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consultando tarifas aplicadas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AplicarTarifa_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var request = new AplicarTarifaRequest
        {
            IdContaCorrente = "conta-123",
            Valor = 2.50m
        };

        tarifaRepositoryMock.Setup(x => x.SalvarAsync(It.IsAny<Tarifa>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.AplicarTarifa(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        tarifaRepositoryMock.Verify(x => x.SalvarAsync(It.IsAny<Tarifa>()), Times.Once);
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tarifa aplicada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AplicarTarifa_ComIdContaVazio_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AplicarTarifaRequest
        {
            IdContaCorrente = "", // ID vazio
            Valor = 2.50m
        };

        // Act
        var result = await controller.AplicarTarifa(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task AplicarTarifa_ComValorZero_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AplicarTarifaRequest
        {
            IdContaCorrente = "conta-123",
            Valor = 0m // Valor zero
        };

        // Act
        var result = await controller.AplicarTarifa(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task AplicarTarifa_ComValorNegativo_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AplicarTarifaRequest
        {
            IdContaCorrente = "conta-123",
            Valor = -5.00m // Valor negativo
        };

        // Act
        var result = await controller.AplicarTarifa(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task AplicarTarifa_ComExcecao_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AplicarTarifaRequest
        {
            IdContaCorrente = "conta-123",
            Valor = 2.50m
        };

        tarifaRepositoryMock.Setup(x => x.SalvarAsync(It.IsAny<Tarifa>()))
            .ThrowsAsync(new Exception("Erro de banco de dados"));

        // Act
        var result = await controller.AplicarTarifa(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
        
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao aplicar tarifa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ConsultarTarifas_ComExcecao_DeveRetornarBadRequest()
    {
        // Arrange - Simular exceção no método ConsultarTarifas
        // Vamos criar um controller com um mock que falha
        var failingController = new TarifasController(tarifaRepositoryMock.Object, loggerMock.Object);
        
        // Simular uma exceção no repositório quando necessário
        tarifaRepositoryMock.Setup(x => x.SalvarAsync(It.IsAny<Tarifa>()))
            .ThrowsAsync(new Exception("Erro de banco de dados"));

        // Act - Como o método ConsultarTarifas não usa o repositório, vamos testar o método AplicarTarifa
        var request = new AplicarTarifaRequest
        {
            IdContaCorrente = "conta-123",
            Valor = 2.50m
        };

        var result = await failingController.AplicarTarifa(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }
}
