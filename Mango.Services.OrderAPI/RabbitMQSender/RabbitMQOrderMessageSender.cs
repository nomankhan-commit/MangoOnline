using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;    
        public RabbitMQOrderMessageSender()
        {
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest"; 
        }

        async void IRabbitMQOrderMessageSender.SendMessage(object message, string exchangeName)
        {
            if (ConnectionExists())
            {
                using var channel = await _connection.CreateChannelAsync();
                //await channel.QueueDeclareAsync(queueName, false, false, false, null);
                await channel.ExchangeDeclareAsync(exchangeName,ExchangeType.Fanout, durable:false);
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "",  body:body);
            }
        }

       async private void  CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password,
                };
                _connection = await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

       private bool ConnectionExists()
        {
                if (_connection!=null)
                {
                    return true;
                }
                else
                {
                    CreateConnection();
                    return true;
                }
        }

    }
}
