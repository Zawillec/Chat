using Chat.Domain.Entities;

namespace Chat.Application.Interfaces
{
    public interface IMessageRepository
    {
        Task AddAsync(Message message);
        Task<IEnumerable<Message>> GetAllAsync();
        Task<IEnumerable<Message>> GetBySenderIdAsync(Guid senderId);
        Task<IEnumerable<Message>> GetByReceiverIdAsync(Guid receiverId);
    }
}
