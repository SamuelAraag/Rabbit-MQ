using MassTransit;
using RabbitMQ.Events.Topic;

namespace RabbitMQ.Consumers.Topic
{
    /// <summary>
    /// Consumer que recebe TODOS os logs (qualquer nível, qualquer módulo)
    /// 
    /// CONCEITO: TOPIC EXCHANGE - WILDCARD #
    /// - Binding Pattern: "log.#"
    /// - O wildcard # significa: "zero ou mais palavras"
    /// - Este consumer receberá QUALQUER mensagem que comece com "log."
    /// 
    /// EXEMPLOS DE ROUTING KEYS QUE ESTE CONSUMER RECEBERÁ:
    /// "log.info.pedidos"         -> recebe
    /// "log.error.pagamentos"     -> recebe
    /// "log.warning.usuarios"     -> recebe
    /// "log.info.api.gateway"     -> recebe (múltiplas palavras)
    /// "payment.error"            -> NÃO recebe (não começa com "log.")
    /// 
    /// CASO DE USO:
    /// - Monitoramento geral do sistema
    /// - Agregação de todos os logs em um lugar central
    /// - Análise e estatísticas de logs
    /// </summary>
    public class AllLogsConsumer : IConsumer<LogEvent>
    {
        private readonly ILogger<AllLogsConsumer> _logger;

        public AllLogsConsumer(ILogger<AllLogsConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<LogEvent> context)
        {
            var log = context.Message;
            var correlationId = context.CorrelationId ?? log.Id;
            
            _logger.LogInformation(
                "[TOPIC - ALL LOGS] Log capturado - CorrelationId: {CorrelationId}, Nível: {Nivel}, Módulo: {Modulo}, Mensagem: {Mensagem}",
                correlationId,
                log.Nivel,
                log.Modulo,
                log.Mensagem
            );

            // Aqui você poderia:
            // - Salvar em banco de dados
            // - Enviar para serviço de monitoramento (Datadog, New Relic, etc)
            // - Gerar métricas e dashboards
            await Task.Delay(100);

            _logger.LogInformation(
                "[TOPIC - ALL LOGS] Log processado - CorrelationId: {CorrelationId}",
                correlationId
            );
        }
    }
}
