using MassTransit;
using RabbitMQ.Events.Direct;

namespace RabbitMQ.Consumers.Direct
{
    /// <summary>
    /// Consumer que processa pedidos criados
    /// 
    /// CONCEITO: CONSUMER (CONSUMIDOR)
    /// - É quem "assina" e processa as mensagens da fila
    /// - Implementa a interface IConsumer<TEvent> do MassTransit
    /// - O método Consume é chamado automaticamente quando uma mensagem chega
    /// 
    /// COMO FUNCIONA:
    /// 1. Producer publica PedidoCriadoEvent no Exchange Direct
    /// 2. Exchange roteia para a fila "pedidos.queue" (routing key: "pedido.criado")
    /// 3. Este consumer está "ouvindo" essa fila
    /// 4. Quando uma mensagem chega, o método Consume é executado
    /// 
    /// BOAS PRÁTICAS:
    /// - Use logging para rastreabilidade
    /// - Use o CorrelationId para tracking (context.CorrelationId)
    /// - Processe de forma assíncrona
    /// - Trate erros adequadamente (retry, dead letter queue)
    /// </summary>
    public class PedidoCriadoConsumer : IConsumer<PedidoCriadoEvent>
    {
        private readonly ILogger<PedidoCriadoConsumer> _logger;

        public PedidoCriadoConsumer(ILogger<PedidoCriadoConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PedidoCriadoEvent> context)
        {
            var pedido = context.Message;
            
            // CORRELATION ID: identificador único para rastrear a mensagem em toda a aplicação
            var correlationId = context.CorrelationId ?? pedido.Id;
            
            _logger.LogInformation(
                "[DIRECT EXCHANGE] Pedido recebido - CorrelationId: {CorrelationId}, Cliente: {Cliente}, Valor: {Valor:C}",
                correlationId,
                pedido.NomeCliente,
                pedido.ValorTotal
            );

            // Simula processamento do pedido (validação, reserva de estoque, etc)
            await Task.Delay(2000);

            _logger.LogInformation(
                "[DIRECT EXCHANGE] Pedido processado com sucesso - CorrelationId: {CorrelationId}",
                correlationId
            );
            
            // IMPORTANTE: Se houver exceção aqui, o MassTransit pode:
            // - Tentar novamente (retry)
            // - Enviar para uma dead letter queue
            // - Configurável no AppExtension.cs
        }
    }
}
