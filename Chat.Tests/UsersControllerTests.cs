using Chat.Api.Controllers;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Chat.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _controller = new UsersController(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredUsers()
        {
            var users = new List<User>
            {
                new() { Id = Guid.NewGuid(), Username = "admin", Role = "Admin", Active = true },
                new() { Id = Guid.NewGuid(), Username = "user1", Role = "User", Active = false }
            };
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var actionResult = await _controller.Get(null, null, "Admin", null);

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(okResult.Value);
            Assert.Single(returned);
            Assert.Equal("Admin", returned.First().Role);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNoContent_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User
            {
                Id = userId,
                Username = "user",
                PasswordHash = "hashed",
                Role = "User",
                Active = true
            };
            var update = new UpdateUserRequest
            {
                Username = "user",
                Role = "Admin",
                Active = false
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateUser(userId, update);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("Admin", existingUser.Role);
            Assert.False(existingUser.Active);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _controller.UpdateUser(userId, new UpdateUserRequest());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.DeleteAsync(user)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteUser(userId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _controller.DeleteUser(userId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
