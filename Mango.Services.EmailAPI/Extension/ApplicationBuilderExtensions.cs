using Mango.Services.EmailAPI.Messaging;
using System.Runtime.CompilerServices;

namespace Mango.Services.EmailAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusConsumer azureServiceBusConsumer { get; set; }

        public static IApplicationBuilder UserAzureServiceBusConsumer(this IApplicationBuilder app)
        {

            azureServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            hostApplicationLife.ApplicationStarted.Register(OnStart);
            hostApplicationLife.ApplicationStopping.Register(OnStop);
            return app;
            //look
        }
        private static void OnStart()
        {
            azureServiceBusConsumer.Start();
        }

        private static void OnStop()
        {
            azureServiceBusConsumer.Stop();
        }
    }
}
