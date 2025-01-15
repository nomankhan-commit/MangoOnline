using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private string ConnectionString = "Endpoint=sb://mangoweb001.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QRSNFnxOn8mi27+N3yCNrJm/VKeBAQrZ8+ASbLk7BA8=";
        public async Task PublishMessage(object message, string topic_queue_name)
        {

            await using var client = new ServiceBusClient(ConnectionString);
            ServiceBusSender sender = client.CreateSender(topic_queue_name);
            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage
                (Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(serviceBusMessage);
            await sender.DisposeAsync();

        }
    }
}
