namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public interface IRabbitMQAuthMessageSender
    {
        void SendMessage(object message, string queueName);
    }
}
