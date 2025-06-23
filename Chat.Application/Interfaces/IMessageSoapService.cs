using System;
using System.ServiceModel;
using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IMessageSoapService
    {
        [OperationContract]
        SendMessageResponse SendMessage(SendMessageRequest message);

        [OperationContract]
        MessageListResponse GetMessagesBySenderId(Guid senderId);

        [OperationContract]
        MessageListResponse GetMessagesByReceiverId(Guid receiverId);
    }
}
