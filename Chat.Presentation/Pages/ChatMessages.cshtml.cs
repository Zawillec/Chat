using Chat.Application.DTOs;
using Chat.Presentation.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace Chat.Presentation.Pages
{
    public class ChatMessagesModel : PageModel
    {
        public List<MessageResponse> Messages { get; set; } = new();
        public Guid CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid userId)
        {
            var token = HttpContext.Session.GetString("token");
            var userIdStr = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr))
                return new EmptyResult();

            CurrentUserId = Guid.Parse(userIdStr);

            var sent = await SoapHelper.GetMessagesBySender(CurrentUserId, token);
            var received = await SoapHelper.GetMessagesByReceiver(CurrentUserId, token);

            Messages = sent
                .Concat(received)
                .Where(m =>
                    (m.SenderId == CurrentUserId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == CurrentUserId))
                .OrderBy(m => m.SentAt)
                .ToList();

            return Partial("_MessageList", Messages);
        }
    }
}
