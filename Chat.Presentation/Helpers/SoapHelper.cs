using Chat.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Chat.Presentation.Helpers
{
    public static class SoapHelper
    {
        private const string ServiceUrl = "http://localhost:5000/Service.svc";

        public static async Task<List<MessageResponse>> GetMessagesBySender(Guid senderId, string token)
        {
            System.Diagnostics.Debug.WriteLine($"[SOAP REQUEST] GetMessagesBySender({senderId}), token: {token}");

            var body = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
        <soapenv:Header/>
        <soapenv:Body>
            <tem:GetMessagesBySenderId>
                <tem:senderId>{senderId}</tem:senderId>
            </tem:GetMessagesBySenderId>
        </soapenv:Body>
    </soapenv:Envelope>";

            return await CallSoapService(body, "GetMessagesBySenderId", token);
        }



        public static async Task<List<MessageResponse>> GetMessagesByReceiver(Guid receiverId, string token)
        {
            System.Diagnostics.Debug.WriteLine($"[SOAP REQUEST] GetMessagesByReceiver({receiverId}), token: {token}");

            var body = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
        <soapenv:Header/>
        <soapenv:Body>
            <tem:GetMessagesByReceiverId>
                <tem:receiverId>{receiverId}</tem:receiverId>
            </tem:GetMessagesByReceiverId>
        </soapenv:Body>
    </soapenv:Envelope>";

            return await CallSoapService(body, "GetMessagesByReceiverId", token);
        }



        public static async Task SendMessage(SendMessageRequest message, string token)
        {
            var body = $@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:tem='http://tempuri.org/'>
                <soapenv:Header/>
                <soapenv:Body>
                    <tem:SendMessage>
                        <tem:message>
                            <tem:Id>{message.Id}</tem:Id>
                            <tem:SenderId>{message.SenderId}</tem:SenderId>
                            <tem:ReceiverId>{message.ReceiverId}</tem:ReceiverId>
                            <tem:Content>{message.Content}</tem:Content>
                            <tem:SentAt>{message.SentAt:o}</tem:SentAt>
                        </tem:message>
                    </tem:SendMessage>
                </soapenv:Body>
            </soapenv:Envelope>";

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, ServiceUrl);
            request.Headers.Add("SOAPAction", "http://tempuri.org/IMessageSoapService/SendMessage");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(body, Encoding.UTF8, "text/xml");

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        private static async Task<List<MessageResponse>> CallSoapService(string soapXml, string action, string token)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, ServiceUrl);
            request.Headers.Add("SOAPAction", $"http://tempuri.org/IMessageSoapService/{action}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");

            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[SOAP RAW RESPONSE] ({action}) {responseString}");


            var doc = XDocument.Parse(responseString);


            XNamespace d4p1 = "http://schemas.datacontract.org/2004/07/Chat.Application.DTOs";

            var messages = doc.Descendants(d4p1 + "MessageResponse")
                .Select(m => new MessageResponse
                {
                    Id = Guid.Parse(m.Element(d4p1 + "Id")?.Value ?? Guid.Empty.ToString()),
                    SenderId = Guid.Parse(m.Element(d4p1 + "SenderId")?.Value ?? Guid.Empty.ToString()),
                    ReceiverId = Guid.Parse(m.Element(d4p1 + "ReceiverId")?.Value ?? Guid.Empty.ToString()),
                    Content = m.Element(d4p1 + "Content")?.Value ?? "",
                    SentAt = DateTime.Parse(m.Element(d4p1 + "SentAt")?.Value ?? DateTime.MinValue.ToString())
                })
                .ToList();

            return messages;
        }
    }
}
