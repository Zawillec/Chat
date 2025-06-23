using Chat.Application.Interfaces;
using Chat.Domain.Entities;

namespace Chat.Infrastructure.Services
{
    public class MessageService
    {
        private readonly IMessageRepository _messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public Task<IEnumerable<Message>> GetAllAsync()
        {
            return _messageRepository.GetAllAsync();
        }

        public Task<IEnumerable<Message>> GetBySenderIdAsync(Guid senderId)
        {
            return _messageRepository.GetBySenderIdAsync(senderId);
        }

        public Task<IEnumerable<Message>> GetByReceiverIdAsync(Guid receiverId)
        {
            return _messageRepository.GetByReceiverIdAsync(receiverId);
        }

        public Task AddAsync(Message message)
        {
            return _messageRepository.AddAsync(message);
        }
    }
}
