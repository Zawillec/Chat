using Microsoft.EntityFrameworkCore;
using Chat.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Chat.Infrastructure.Persistence
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
