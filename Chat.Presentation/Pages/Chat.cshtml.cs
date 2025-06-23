using Chat.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using Chat.Presentation.Helpers;

namespace Chat.Presentation.Pages
{
    public class ChatModel : PageModel
    {
        public Guid CurrentUserId { get; set; }
        public Guid SelectedUserId { get; set; }
        public string ReceiverUsername { get; set; }
        public List<MessageResponse> Messages { get; set; } = new();
        public List<UserResponse> Users { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("token");
            var userIdStr = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            CurrentUserId = Guid.Parse(userIdStr);
            SelectedUserId = Guid.Empty;
            Messages = new();
            ReceiverUsername = "";

            var userIdParam = Request.Query["userId"].FirstOrDefault();
            if (Guid.TryParse(userIdParam, out Guid selectedUserId) && selectedUserId != Guid.Empty)
            {
                SelectedUserId = selectedUserId;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var result = await client.GetFromJsonAsync<List<UserResponse>>("http://localhost:5000/api/Users");
            Users = result ?? new();

            if (SelectedUserId != Guid.Empty)
            {
                var messages = await GetConversation(CurrentUserId, SelectedUserId, token);
                Messages = messages.OrderBy(m => m.SentAt).ToList();

                ReceiverUsername = Users.FirstOrDefault(u => u.Id == SelectedUserId)?.Username ?? "";
            }

            return Page();
        }




        public async Task<IActionResult> OnPostAsync(string content, Guid receiverId)
        {
            var token = HttpContext.Session.GetString("token");
            var userIdStr = HttpContext.Session.GetString("userId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            if (receiverId == Guid.Empty || string.IsNullOrWhiteSpace(content))
                return RedirectToPage("Chat", new { userId = receiverId });

            var senderId = Guid.Parse(userIdStr);

            var message = new SendMessageRequest
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            await SoapHelper.SendMessage(message, token);

            return RedirectToPage("Chat", new { userId = receiverId });
        }

        private async Task<List<MessageResponse>> GetConversation(Guid currentUser, Guid otherUser, string token)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] GET CONVERSATION wywołany");

            var senderMessages = await SoapHelper.GetMessagesBySender(currentUser, token);
            var receivedMessages = await SoapHelper.GetMessagesByReceiver(currentUser, token);

            var allMessages = senderMessages.Concat(receivedMessages).ToList();

            foreach (var msg in allMessages)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] MSG: {msg.SenderId} → {msg.ReceiverId} | {msg.Content}");
            }

            var conversation = allMessages
                .Where(m =>
                {
                    bool match =
                        (m.SenderId == currentUser && m.ReceiverId == otherUser) ||
                        (m.SenderId == otherUser && m.ReceiverId == currentUser);

                    if (!match)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ODRZUCONE] {m.SenderId} → {m.ReceiverId} ≠ ({currentUser} <→> {otherUser})");
                    }

                    return match;
                })
                .OrderBy(m => m.SentAt)
                .ToList();

            return conversation;
        }


    }
}
