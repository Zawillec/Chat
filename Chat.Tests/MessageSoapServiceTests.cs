using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Chat.Tests
{
    public class MessageSoapServiceTests
    {
        private readonly Mock<IMessageRepository> _messageRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly MessageSoapService _service;

        private readonly Guid _userId = Guid.NewGuid();

        public MessageSoapServiceTests()
        {
            _messageRepoMock = new Mock<IMessageRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
            }, "TestAuth");

            var user = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = user };
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            _service = new MessageSoapService(_messageRepoMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public void SendMessage_ShouldSucceed_WhenSenderMatchesUser()
        {
            var message = new SendMessageRequest
            {
                Id = Guid.NewGuid(),
                SenderId = _userId,
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!",
                SentAt = DateTime.UtcNow
            };

            var result = _service.SendMessage(message);

            _messageRepoMock.Verify(r => r.AddAsync(It.IsAny<Message>()), Times.Once);
            Assert.True(result.Success);
            Assert.Equal("Message sent successfully.", result.Message);
        }

        [Fact]
        public void SendMessage_ShouldFail_WhenSenderDoesNotMatchUser()
        {
            var message = new SendMessageRequest
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Bad",
                SentAt = DateTime.UtcNow
            };

            var result = _service.SendMessage(message);

            _messageRepoMock.Verify(r => r.AddAsync(It.IsAny<Message>()), Times.Never);
            Assert.False(result.Success);
        }

        [Fact]
        public void GetMessagesBySenderId_ShouldReturnMessages_WhenAuthorized()
        {
            var messages = new List<Message>
            {
                new Message { Id = Guid.NewGuid(), SenderId = _userId, ReceiverId = Guid.NewGuid(), Content = "Test", SentAt = DateTime.UtcNow }
            };
            _messageRepoMock.Setup(r => r.GetBySenderIdAsync(_userId)).ReturnsAsync(messages);

            var result = _service.GetMessagesBySenderId(_userId);

            Assert.True(result.Success);
            Assert.Single(result.Messages);
        }

        [Fact]
        public void GetMessagesBySenderId_ShouldFail_WhenUnauthorized()
        {
            var result = _service.GetMessagesBySenderId(Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Empty(result.Messages);
        }

        [Fact]
        public void GetMessagesByReceiverId_ShouldReturnMessages_WhenAuthorized()
        {
            var messages = new List<Message>
            {
                new Message { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), ReceiverId = _userId, Content = "Hi", SentAt = DateTime.UtcNow }
            };
            _messageRepoMock.Setup(r => r.GetByReceiverIdAsync(_userId)).ReturnsAsync(messages);

            var result = _service.GetMessagesByReceiverId(_userId);

            Assert.True(result.Success);
            Assert.Single(result.Messages);
        }

        [Fact]
        public void GetMessagesByReceiverId_ShouldFail_WhenUnauthorized()
        {
            var result = _service.GetMessagesByReceiverId(Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Empty(result.Messages);
        }
    }
}
