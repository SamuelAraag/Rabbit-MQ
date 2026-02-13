using MassTransit;
using RabbitMQ.Consumers.Direct;
using RabbitMQ.Consumers.Fanout;
using RabbitMQ.Consumers.Topic;
using RabbitMQ.Models;

namespace RabbitMQ.Extensions
{
    /// <summary>
    /// Extension methods para configurar os serviços do RabbitMQ
    /// </summary>
    public static class AppExtension
    {
        /// <summary>
        /// Configura o MassTransit com RabbitMQ
        /// 
        /// O QUE É O MASSTRANSIT?
        /// - É uma biblioteca que abstrai a complexidade do RabbitMQ
        /// - Facilita a criação de producers e consumers
        /// - Gerencia automaticamente conexões, filas, exchanges
        /// - Fornece recursos como retry, circuit breaker, etc.
        /// 
        /// ESTE MÉTODO FAZ:
        /// 1. Registra todos os consumers (quem vai processar as mensagens)
        /// 2. Configura conexão com RabbitMQ
        /// 3. MassTransit cria automaticamente os exchanges e filas necessários
        /// </summary>
        public static void AddRabbitMQService(
            this IServiceCollection services, 
            RabbitMQSettings settings)
        {
            services.AddMassTransit(busConfigurator =>
            {
                // ===== REGISTRANDO CONSUMERS =====
                // Cada AddConsumer informa ao MassTransit que temos um consumer
                // O MassTransit criará automaticamente as filas necessárias
                
                // Consumer do Direct Exchange
                busConfigurator.AddConsumer<PedidoCriadoConsumer>();
                
                // Consumers do Fanout Exchange (3 consumers para broadcast)
                busConfigurator.AddConsumer<EmailConsumer>();
                busConfigurator.AddConsumer<SMSConsumer>();
                busConfigurator.AddConsumer<PushNotificationConsumer>();
                
                // Consumers do Topic Exchange (2 consumers com patterns diferentes)
                busConfigurator.AddConsumer<AllLogsConsumer>();
                busConfigurator.AddConsumer<ErrorLogsConsumer>();

                // ===== CONFIGURANDO RABBITMQ =====
                busConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    // Configuração de conexão (lida do appsettings.json)
                    cfg.Host(settings.Host, h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });

                    // ConfigureEndpoints cria automaticamente todas as filas e bindings
                    // para os consumers registrados acima
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}