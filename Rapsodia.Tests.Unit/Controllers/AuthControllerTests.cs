using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Rapsodia.Blue.Presentation.Controllers;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Tests.Unit.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidCredentials_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "thierreteste",
            Email = "user@abitat.com",
            Password = "SecurePassword123",
            FullName = "Thierre Nome Completo"
        };

        var authResult = new AuthResultDTO
        {
            UserId = 1,
            Username = "thierreteste",
            Token = "jwt-token-exemplo",
            RefreshToken = "refresh-token-exemplo",
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };

        var expectedResult = Result<AuthResultDTO>.Ok(authResult);

        _authServiceMock
            .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert - Register retorna CreatedAtAction (201)
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "th1eros",
            Password = "SecurePassword123"
        };

        var authResult = new AuthResultDTO
        {
            UserId = 1,
            Username = "th1eros",
            Token = "jwt-token-exemplo",
            RefreshToken = "refresh-token-exemplo",
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };

        var expectedResult = Result<AuthResultDTO>.Ok(authResult);

        _authServiceMock
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert - Login retorna Result<AuthResultDTO> dentro do Ok
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<Result<AuthResultDTO>>(okResult.Value);
        Assert.True(value.Success);
        Assert.NotNull(value.Data);
        Assert.Equal("th1eros", value.Data.Username);
    }

    [Fact]
    public async Task Register_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Required");

        var request = new RegisterRequest
        {
            Username = "teste",
            Email = "",
            Password = "SecurePassword123",
            FullName = "Teste"
        };

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "th1eros",
            Password = "WrongPassword"
        };

        var errorResult = Result<AuthResultDTO>.Fail("Credenciais invalidas");

        _authServiceMock
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert - Retorna Result<AuthResultDTO> com Success=false
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var value = Assert.IsType<Result<AuthResultDTO>>(unauthorizedResult.Value);
        Assert.False(value.Success);
        Assert.Equal("Credenciais invalidas", value.Message);
    }
}