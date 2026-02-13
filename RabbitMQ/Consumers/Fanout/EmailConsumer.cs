using MassTransit;
using RabbitMQ.Events.Fanout;

namespace RabbitMQ.Consumers.Fanout
{
    /// <summary>
    /// Consumer que processa notificações via EMAIL
    /// 
    /// CONCEITO: FANOUT EXCHANGE - MÚLTIPLOS CONSUMERS
    /// - Este é UM dos três consumers que receberão a MESMA mensagem
    /// - Fanout = Broadcast = todos recebem uma cópia
    /// - Cada consumer processa de forma INDEPENDENTE e PARALELA
    /// 
    /// NESTE CENÁRIO:
    /// - Producer publica NotificacaoGeralEvent no Exchange Fanout
    /// - Exchange COPIA a mensagem para 3 filas:
    ///   1. notificacao.email.queue -> EmailConsumer (este)
    ///   2. notificacao.sms.queue -> SMSConsumer
    ///   3. notificacao.push.queue -> PushNotificationConsumer
    /// - Todos processam simultaneamente a mesma notificação
    /// </summary>
    public class EmailConsumer : IConsumer<NotificacaoGeralEvent>
    {
        private readonly ILogger<EmailConsumer> _logger;

        public EmailConsumer(ILogger<EmailConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<NotificacaoGeralEvent> context)
        {
            var notificacao = context.Message;
            var correlationId = context.CorrelationId ?? notificacao.Id;
            
            _logger.LogInformation(
                "[FANOUT - EMAIL] Enviando email - CorrelationId: {CorrelationId}, Para: {Destinatario}, Assunto: {Titulo}",
                correlationId,
                notificacao.Destinatario,
                notificacao.Titulo
            );

            // Simula envio de email (integração com SendGrid, AWS SES, etc)
            await Task.Delay(1500);

            _logger.LogInformation(
                "[FANOUT - EMAIL] Email enviado com sucesso - CorrelationId: {CorrelationId}",
                correlationId
            );
        }
    }
}
