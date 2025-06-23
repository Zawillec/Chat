using System;
using System.Runtime.Serialization;

namespace Chat.Application.DTOs
{
    [DataContract(Namespace = "http://tempuri.org/")]
    public class SendMessageRequest
    {
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 2)]
        public Guid SenderId { get; set; }

        [DataMember(Order = 3)]
        public Guid ReceiverId { get; set; }

        [DataMember(Order = 4)]
        public string Content { get; set; }

        [DataMember(Order = 5)]
        public DateTime SentAt { get; set; }
    }
}
