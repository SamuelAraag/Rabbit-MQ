using MassTransit;
using RabbitMQ.Events.Fanout;

namespace RabbitMQ.Consumers.Fanout
{
    /// <summary>
    /// Consumer que processa notificações via Push Notification
    /// 
    /// FANOUT EXCHANGE - Consumer 3 de 3
    /// - Recebe a MESMA mensagem que EmailConsumer e SMSConsumer
    /// - Todos os três processam SIMULTANEAMENTE
    /// - Exemplo prático: "Promoção relâmpago!" -> envia por Email + SMS + Push ao mesmo tempo
    /// </summary>
    public class PushNotificationConsumer : IConsumer<NotificacaoGeralEvent>
    {
        private readonly ILogger<PushNotificationConsumer> _logger;

        public PushNotificationConsumer(ILogger<PushNotificationConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<NotificacaoGeralEvent> context)
        {
            var notificacao = context.Message;
            var correlationId = context.CorrelationId ?? notificacao.Id;
            
            _logger.LogInformation(
                "[FANOUT - PUSH] Enviando push notification - CorrelationId: {CorrelationId}, Para: {Destinatario}, Titulo: {Titulo}",
                correlationId,
                notificacao.Destinatario,
                notificacao.Titulo
            );

            // Simula envio de push notification (integração com Firebase, OneSignal, etc)
            await Task.Delay(800);

            _logger.LogInformation(
                "[FANOUT - PUSH] Push notification enviado com sucesso - CorrelationId: {CorrelationId}",
                correlationId
            );
        }
    }
}
