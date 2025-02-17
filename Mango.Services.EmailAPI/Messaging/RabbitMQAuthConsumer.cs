
using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
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
            await channel.QueueDeclareAsync(_configuration.GetValue<string>("TopicsAndQueueName:RegisterUserQueue"), false, false, false, null);

            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(channel);
           consumer.ReceivedAsync += async  (ch, ea) =>
           {

                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                string email = JsonConvert.DeserializeObject<string>(content);  
                HandleMessage(email).GetAwaiter().GetResult();
                channel.BasicAckAsync(ea.DeliveryTag, false);
            };
            await channel.BasicConsumeAsync(_configuration.GetValue<string>("TopicsAndQueueName:RegisterUserQueue"),false, consumer);

            return Task.CompletedTask;


        }

        private async Task HandleMessage(string email) {

            _emailService.RegisterUserEmailandLog(email).GetAwaiter().GetResult();

        }
    }
}
