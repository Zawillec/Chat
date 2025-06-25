using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Application.Settings;
using Chat.Domain.Entities;
using Chat.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chat.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly IAuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();

            var jwtSettings = new JwtSettings
            {
                Key = "Xn98!2f#KqLmZt5pYrGhBvDd*7bGtR^Q",
                Issuer = "ChatApp",
                Audience = "ChatUsers"
            };

            var options = Options.Create(jwtSettings);
            _authService = new AuthService(_userRepositoryMock.Object, options);
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegister_WhenUserDoesNotExist()
        {
            var request = new RegisterRequest
            {
                Username = "newuser",
                Password = "Password123",
                Role = "User"
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("newuser"))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            Assert.NotNull(result);
            Assert.Equal("newuser", result.Username);
            Assert.Equal("User", result.Role);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUserExists()
        {
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "existinguser",
                PasswordHash = "hashed",
                Role = "User"
            };

            var request = new RegisterRequest
            {
                Username = "existinguser",
                Password = "whatever",
                Role = "User"
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("existinguser"))
                .ReturnsAsync(existingUser);

            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(request));
            Assert.Equal("User already exists.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var request = new LoginRequest
            {
                Username = "validuser",
                Password = "secret"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "validuser",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("secret")),
                Role = "User",
                Active = true
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("validuser"))
                .ReturnsAsync(user);

            var result = await _authService.LoginAsync(request);

            Assert.NotNull(result);
            Assert.Equal("validuser", result.Username);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenCredentialsAreInvalid()
        {
            var request = new LoginRequest
            {
                Username = "wronguser",
                Password = "wrongpass"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "wronguser",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("notcorrect")),
                Role = "User",
                Active = true
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("wronguser"))
                .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(request));
            Assert.Equal("Invalid credentials.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenAccountIsDeactivated()
        {
            var request = new LoginRequest
            {
                Username = "deactivated",
                Password = "password"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "deactivated",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password")),
                Role = "User",
                Active = false
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("deactivated"))
                .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(request));
            Assert.Equal("Account is deactivated.", ex.Message);
        }
    }
}
