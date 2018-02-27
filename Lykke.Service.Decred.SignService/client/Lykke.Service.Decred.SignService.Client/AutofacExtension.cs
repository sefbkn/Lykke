using System;
using Autofac;
using Common.Log;

namespace Lykke.Service.Decred_SignService.Client
{
    public static class AutofacExtension
    {
        public static void RegisterDecred_SignServiceClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<Decred_SignServiceClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IDecred_SignServiceClient>()
                .SingleInstance();
        }

        public static void RegisterDecred_SignServiceClient(this ContainerBuilder builder, Decred_SignServiceServiceClientSettings settings, ILog log)
        {
            builder.RegisterDecred_SignServiceClient(settings?.ServiceUrl, log);
        }
    }
}
