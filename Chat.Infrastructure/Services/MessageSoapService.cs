using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Chat.Infrastructure.Services
{
    public class MessageSoapService : IMessageSoapService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MessageSoapService(IMessageRepository messageRepository, IHttpContextAccessor httpContextAccessor)
        {
            _messageRepository = messageRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public SendMessageResponse SendMessage(SendMessageRequest message)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty || userId != message.SenderId)
            {
                return new SendMessageResponse
                {
                    Success = false,
                    Message = "Forbidden – sender mismatch or unauthenticated."
                };
            }

            var entity = new Message
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                SentAt = message.SentAt
            };

            _messageRepository.AddAsync(entity).GetAwaiter().GetResult();

            return new SendMessageResponse
            {
                Success = true,
                Message = "Message sent successfully."
            };
        }

        public MessageListResponse GetMessagesBySenderId(Guid senderId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty || userId != senderId)
            {
                return new MessageListResponse
                {
                    Success = false,
                    Message = "Forbidden – sender mismatch or unauthenticated.",
                    Messages = new()
                };
            }

            var result = _messageRepository.GetBySenderIdAsync(senderId).GetAwaiter().GetResult();
            return new MessageListResponse
            {
                Success = true,
                Message = "OK",
                Messages = result.Select(m => new MessageResponse
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    SentAt = m.SentAt
                }).ToList()
            };
        }

        public MessageListResponse GetMessagesByReceiverId(Guid receiverId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty || userId != receiverId)
            {
                return new MessageListResponse
                {
                    Success = false,
                    Message = "Forbidden – receiver mismatch or unauthenticated.",
                    Messages = new()
                };
            }

            var result = _messageRepository.GetByReceiverIdAsync(receiverId).GetAwaiter().GetResult();
            return new MessageListResponse
            {
                Success = true,
                Message = "OK",
                Messages = result.Select(m => new MessageResponse
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    SentAt = m.SentAt
                }).ToList()
            };
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);

            if (user?.Identity?.IsAuthenticated != true || userIdClaim == null)
                return Guid.Empty;

            return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }
    }
}
