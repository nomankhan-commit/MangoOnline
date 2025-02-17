
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
             _configuration = configuration;
            _emailService = emailService;
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest";
        }
        protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
        {
           
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };
            _connection = await factory.CreateConnectionAsync();
            using var channel = await _connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(_configuration.GetValue<string>("TopicsAndQueueName:OrderCreatedTopic"), ExchangeType.Fanout);
            var queue = await channel.QueueDeclareAsync();
            var queueName = queue.QueueName;
            channel.QueueBindAsync(queueName, _configuration.GetValue<string>("TopicsAndQueueName:OrderCreatedTopic"),"");
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(channel);
           consumer.ReceivedAsync += async  (ch, ea) =>
           {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
               RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);  
                HandleMessage(rewardsMessage).GetAwaiter().GetResult();
                channel.BasicAckAsync(ea.DeliveryTag, false);
            };
            await channel.BasicConsumeAsync(_configuration.GetValue<string>("TopicsAndQueueName:OrderCreatedTopic"),false, consumer);

            return Task.CompletedTask;


        }

        private async Task HandleMessage(RewardsMessage rewardsMessage) {

            _emailService.LogPlacedOrder(rewardsMessage).GetAwaiter().GetResult();

        }
    }
}
