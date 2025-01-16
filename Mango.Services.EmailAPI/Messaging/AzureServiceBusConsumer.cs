using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _serviceBusOrderConnectionString;
        private readonly string _emailCartQueue;
        private readonly string _registerUserQueue;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor _emailCartProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            //_serviceBusOrderConnectionString = _configuration.GetValue<string>("ServiceBusOrderConnectionString");
            _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueName:EmailShoppingCart");
           // _orderCreated_Topic = _configuration.GetValue<string>("TopicAndQueueName:OrderCreatedTopic");
            //_orderCreated_Email_Subscription = _configuration.GetValue<string>("TopicAndQueueName:OrderCreated_Reward_Subscription");

            var client = new ServiceBusClient(_serviceBusConnectionString);
            _emailCartProcessor = client.CreateProcessor(_emailCartQueue);
            
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;//look
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();

        }
        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();
        }
        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                //await _emailService.EmailCartLog(cartDto);
                args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
