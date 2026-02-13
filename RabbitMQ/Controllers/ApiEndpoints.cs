using MassTransit;
using RabbitMQ.Events.Direct;
using RabbitMQ.Events.Fanout;
using RabbitMQ.Events.Topic;

namespace RabbitMQ.Controllers
{
    /// <summary>
    /// Endpoints da API para testar os diferentes tipos de Exchange
    /// 
    /// CONCEITO: PRODUCER (PRODUTOR)
    /// - É quem CRIA e PUBLICA as mensagens
    /// - Não sabe quem vai consumir (desacoplamento)
    /// - Apenas envia pro Exchange e o Exchange roteia
    /// </summary>
    public static class ApiEndpoints
    {
        public static void AddApiEndpoint(this WebApplication app)
        {
            // ===== ENDPOINT: DIRECT EXCHANGE =====
            /// <summary>
            /// POST /api/pedido
            /// Demonstra o uso de DIRECT EXCHANGE
            /// 
            /// O QUE ACONTECE:
            /// 1. Cria um PedidoCriadoEvent
            /// 2. Publica no Exchange Direct (marketplace.direct)
            /// 3. Exchange roteia para fila "pedidos.queue" (routing key exata: "pedido.criado")
            /// 4. PedidoCriadoConsumer recebe e processa
            /// 
            /// TESTE NO SWAGGER:
            /// {
            ///   "nomeCliente": "João Silva",
            ///   "valorTotal": 299.90
            /// }
            /// </summary>
            app.MapPost("/api/pedido", async (PedidoRequest request, IBus bus, ILogger<Program> logger) =>
            {
                var pedido = new PedidoCriadoEvent(
                    Id: Guid.NewGuid(),
                    NomeCliente: request.NomeCliente,
                    ValorTotal: request.ValorTotal,
                    DataCriacao: DateTime.Now
                );

                // IMPORTANTE: SetRoutingKey define a routing key para Direct Exchange
                await bus.Publish(pedido, ctx =>
                {
                    ctx.SetRoutingKey("pedido.criado"); // Routing key EXATA
                });

                logger.LogInformation(
                    "[PRODUCER - DIRECT] Pedido publicado - Id: {Id}, Cliente: {Cliente}",
                    pedido.Id,
                    pedido.NomeCliente
                );

                return Results.Ok(new
                {
                    Message = "Pedido criado e enviado para processamento!",
                    PedidoId = pedido.Id,
                    ExchangeType = "DIRECT",
                    ExchangeName = "marketplace.direct",
                    RoutingKey = "pedido.criado",
                    Dica = "Verifique os logs do consumer para ver o processamento"
                });
            })
            .WithName("CriarPedido");

            // ===== ENDPOINT: FANOUT EXCHANGE =====
            /// <summary>
            /// POST /api/notificacao
            /// Demonstra o uso de FANOUT EXCHANGE (Broadcast)
            /// 
            /// O QUE ACONTECE:
            /// 1. Cria um NotificacaoGeralEvent
            /// 2. Publica no Exchange Fanout (notifications.fanout)
            /// 3. Exchange COPIA a mensagem para 3 filas:
            ///    - notificacao.email.queue -> EmailConsumer
            ///    - notificacao.sms.queue -> SMSConsumer
            ///    - notificacao.push.queue -> PushNotificationConsumer
            /// 4. Os 3 consumers processam SIMULTANEAMENTE
            /// 
            /// TESTE NO SWAGGER:
            /// {
            ///   "titulo": "Promoção Relâmpago!",
            ///   "mensagem": "50% de desconto em todos os produtos",
            ///   "destinatario": "Maria Santos"
            /// }
            /// </summary>
            app.MapPost("/api/notificacao", async (NotificacaoRequest request, IBus bus, ILogger<Program> logger) =>
            {
                var notificacao = new NotificacaoGeralEvent(
                    Id: Guid.NewGuid(),
                    Titulo: request.Titulo,
                    Mensagem: request.Mensagem,
                    Destinatario: request.Destinatario,
                    DataEnvio: DateTime.Now
                );

                // FANOUT não usa routing key - envia para TODOS
                await bus.Publish(notificacao);

                logger.LogInformation(
                    "[PRODUCER - FANOUT] Notificação publicada - Id: {Id}, Para: {Destinatario}",
                    notificacao.Id,
                    notificacao.Destinatario
                );

                return Results.Ok(new
                {
                    Message = "Notificação enviada para todos os canais (Email, SMS, Push)!",
                    NotificacaoId = notificacao.Id,
                    ExchangeType = "FANOUT (Broadcast)",
                    ExchangeName = "notifications.fanout",
                    Canais = new[] { "Email", "SMS", "Push Notification" },
                    Dica = "Verifique os logs - você verá 3 consumers processando a mesma mensagem"
                });
            })
            .WithName("EnviarNotificacao");

            // ===== ENDPOINT: TOPIC EXCHANGE =====
            /// <summary>
            /// POST /api/log
            /// Demonstra o uso de TOPIC EXCHANGE (Pattern Matching)
            /// 
            /// O QUE ACONTECE:
            /// 1. Cria um LogEvent
            /// 2. Gera routing key no formato: "log.{nivel}.{modulo}"
            /// 3. Publica no Exchange Topic (logs.topic)
            /// 4. Exchange roteia baseado em PADRÕES:
            ///    - AllLogsConsumer (pattern: "log.#") -> recebe TODOS
            ///    - ErrorLogsConsumer (pattern: "log.error.#") -> recebe APENAS erros
            /// 
            /// TESTES NO SWAGGER:
            /// 
            /// Teste 1 - Log de INFO (vai apenas para AllLogsConsumer):
            /// {
            ///   "nivel": "info",
            ///   "modulo": "pedidos",
            ///   "mensagem": "Pedido criado com sucesso"
            /// }
            /// 
            /// Teste 2 - Log de ERROR (vai para AMBOS os consumers):
            /// {
            ///   "nivel": "error",
            ///   "modulo": "pagamentos",
            ///   "mensagem": "Falha ao processar pagamento"
            /// }
            /// 
            /// Teste 3 - Log de WARNING (vai apenas para AllLogsConsumer):
            /// {
            ///   "nivel": "warning",
            ///   "modulo": "usuarios",
            ///   "mensagem": "Tentativa de login suspeita"
            /// }
            /// </summary>
            app.MapPost("/api/log", async (LogRequest request, IBus bus, ILogger<Program> logger) =>
            {
                var log = new LogEvent(
                    Id: Guid.NewGuid(),
                    Nivel: request.Nivel.ToLower(),
                    Modulo: request.Modulo.ToLower(),
                    Mensagem: request.Mensagem,
                    DataHora: DateTime.Now
                );

                var routingKey = log.GetRoutingKey(); // "log.{nivel}.{modulo}"

                // TOPIC usa routing key com pattern
                await bus.Publish(log, ctx =>
                {
                    ctx.SetRoutingKey(routingKey);
                });

                logger.LogInformation(
                    "[PRODUCER - TOPIC] Log publicado - Id: {Id}, RoutingKey: {RoutingKey}",
                    log.Id,
                    routingKey
                );

                // Determina quais consumers vão receber
                var consumersQueVaoReceber = new List<string> { "AllLogsConsumer (log.#)" };
                if (request.Nivel.ToLower() == "error")
                {
                    consumersQueVaoReceber.Add("ErrorLogsConsumer (log.error.#)");
                }

                return Results.Ok(new
                {
                    Message = "Log registrado e publicado!",
                    LogId = log.Id,
                    ExchangeType = "TOPIC (Pattern Matching)",
                    ExchangeName = "logs.topic",
                    RoutingKey = routingKey,
                    ConsumersQueVaoReceber = consumersQueVaoReceber,
                    Dica = request.Nivel.ToLower() == "error"
                        ? "Este é um ERROR - vai para 2 consumers!"
                        : "Este NÃO é um error - vai apenas para AllLogsConsumer"
                });
            })
            .WithName("RegistrarLog");

            // ===== ENDPOINT: STATUS =====
            /// <summary>
            /// GET /api/status
            /// Retorna informações sobre a configuração do RabbitMQ
            /// </summary>
            app.MapGet("/api/status", () =>
            {
                return Results.Ok(new
                {
                    Message = "RabbitMQ Study Project - Funcionando!",
                    Endpoints = new
                    {
                        DirectExchange = "POST /api/pedido",
                        FanoutExchange = "POST /api/notificacao",
                        TopicExchange = "POST /api/log"
                    },
                    Exchanges = new[]
                    {
                        new { Tipo = "DIRECT", Nome = "marketplace.direct", Conceito = "Routing key exata" },
                        new { Tipo = "FANOUT", Nome = "notifications.fanout", Conceito = "Broadcast para todos" },
                        new { Tipo = "TOPIC", Nome = "logs.topic", Conceito = "Pattern matching com wildcards" }
                    },
                    RabbitMQInterface = "http://localhost:15672 (guest/guest)"
                });
            })
            .WithName("Status");
        }
    }

    // ===== DTOs (Data Transfer Objects) =====
    // Estas classes definem o formato dos dados recebidos nos endpoints

    public record PedidoRequest(string NomeCliente, decimal ValorTotal);
    public record NotificacaoRequest(string Titulo, string Mensagem, string Destinatario);
    public record LogRequest(string Nivel, string Modulo, string Mensagem);
}
