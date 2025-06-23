using System;
using System.Runtime.Serialization;

namespace Chat.Domain.Entities
{
    [DataContract]
    public class Message
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
