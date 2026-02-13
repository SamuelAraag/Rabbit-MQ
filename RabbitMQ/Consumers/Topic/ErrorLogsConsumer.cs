using MassTransit;
using RabbitMQ.Events.Topic;

namespace RabbitMQ.Consumers.Topic
{
    /// <summary>
    /// Consumer que recebe APENAS logs de ERRO
    /// 
    /// CONCEITO: TOPIC EXCHANGE - PATTERN ESPECÍFICO
    /// - Binding Pattern: "log.error.#"
    /// - Este consumer é mais SELETIVO que o AllLogsConsumer
    /// - Receberá apenas mensagens que começam com "log.error."
    /// 
    /// EXEMPLOS DE ROUTING KEYS:
    /// ❌ "log.info.pedidos"         -> NÃO recebe (não é error)
    /// ✅ "log.error.pagamentos"     -> recebe
    /// ❌ "log.warning.usuarios"     -> NÃO recebe (não é error)
    /// ✅ "log.error.api.gateway"    -> recebe
    /// ✅ "log.error.database"       -> recebe
    /// 
    /// CASO DE USO:
    /// - Alertas críticos e notificações
    /// - Sistema de on-call (chamar desenvolvedor quando houver erro)
    /// - Tracking de bugs em produção
    /// - Integração com PagerDuty, Slack, etc.
    /// 
    /// COMPARAÇÃO COM AllLogsConsumer:
    /// - AllLogsConsumer: "log.#" -> recebe TUDO
    /// - ErrorLogsConsumer: "log.error.#" -> recebe APENAS erros
    /// - Ambos podem rodar simultaneamente na mesma aplicação!
    /// </summary>
    public class ErrorLogsConsumer : IConsumer<LogEvent>
    {
        private readonly ILogger<ErrorLogsConsumer> _logger;

        public ErrorLogsConsumer(ILogger<ErrorLogsConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<LogEvent> context)
        {
            var log = context.Message;
            var correlationId = context.CorrelationId ?? log.Id;
            
            // Log com nível WARNING para destacar que é um erro crítico
            _logger.LogWarning(
                "[TOPIC - ERROR LOGS] 🚨 ERRO DETECTADO - CorrelationId: {CorrelationId}, Módulo: {Modulo}, Mensagem: {Mensagem}",
                correlationId,
                log.Modulo,
                log.Mensagem
            );

            // Aqui você poderia:
            // - Enviar email para equipe de desenvolvimento
            // - Criar ticket no Jira automaticamente
            // - Enviar alerta no Slack
            // - Acionar sistema de on-call
            await Task.Delay(500);

            _logger.LogWarning(
                "[TOPIC - ERROR LOGS] Alerta de erro processado - CorrelationId: {CorrelationId}",
                correlationId
            );
        }
    }
}
