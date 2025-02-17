﻿using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQAuthMessageSender : IRabbitMQAuthMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;    
        public RabbitMQAuthMessageSender()
        {
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest"; 
        }

        async void IRabbitMQAuthMessageSender.SendMessage(object message, string queueName)
        {
            if (ConnectionExists())
            {
                using var channel = await _connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queueName, false, false, false, null);
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            }
        }

        private void  CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password,
                };
                _connection =  factory.CreateConnectionAsync().GetAwaiter().GetResult();
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
