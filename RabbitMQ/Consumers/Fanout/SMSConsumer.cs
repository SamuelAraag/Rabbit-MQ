using MassTransit;
using RabbitMQ.Events.Fanout;

namespace RabbitMQ.Consumers.Fanout
{
    /// <summary>
    /// Consumer que processa notificações via SMS
    /// 
    /// FANOUT EXCHANGE - Consumer 2 de 3
    /// - Recebe a MESMA mensagem que EmailConsumer e PushNotificationConsumer
    /// - Processa de forma independente e paralela
    /// - Se este consumer falhar, não afeta os outros
    /// </summary>
    public class SMSConsumer : IConsumer<NotificacaoGeralEvent>
    {
        private readonly ILogger<SMSConsumer> _logger;

        public SMSConsumer(ILogger<SMSConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<NotificacaoGeralEvent> context)
        {
            var notificacao = context.Message;
            var correlationId = context.CorrelationId ?? notificacao.Id;
            
            _logger.LogInformation(
                "[FANOUT - SMS] Enviando SMS - CorrelationId: {CorrelationId}, Para: {Destinatario}",
                correlationId,
                notificacao.Destinatario
            );

            // Simula envio de SMS (integração com Twilio, AWS SNS, etc)
            await Task.Delay(1000);

            _logger.LogInformation(
                "[FANOUT - SMS] SMS enviado com sucesso - CorrelationId: {CorrelationId}",
                correlationId
            );
        }
    }
}
