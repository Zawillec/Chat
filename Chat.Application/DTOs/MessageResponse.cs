using System;
using System.Runtime.Serialization;

namespace Chat.Application.DTOs
{
    [DataContract]
    public class MessageResponse
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid SenderId { get; set; }

        [DataMember]
        public Guid ReceiverId { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
