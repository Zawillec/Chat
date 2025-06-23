using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _dbContext;

        public MessageRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Message message)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await _dbContext.Messages.ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetBySenderIdAsync(Guid senderId)
        {
            return await _dbContext.Messages
                .Where(m => m.SenderId == senderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByReceiverIdAsync(Guid receiverId)
        {
            return await _dbContext.Messages
                .Where(m => m.ReceiverId == receiverId)
                .ToListAsync();
        }
    }
}
