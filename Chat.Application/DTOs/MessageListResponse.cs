using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Application.DTOs
{
    public class MessageListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<MessageResponse> Messages { get; set; } = new();
    }
}
