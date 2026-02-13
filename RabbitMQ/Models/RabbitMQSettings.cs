namespace RabbitMQ.Models
{
    /// <summary>
    /// Classe que mapeia as configurações do RabbitMQ do appsettings.json
    /// Isso permite que a aplicação leia as configurações de forma tipada e segura
    /// </summary>
    public class RabbitMQSettings
    {
        /// <summary>
        /// URI de conexão com o RabbitMQ
        /// Formato: amqp://host:porta
        /// Exemplo: amqp://localhost:5672
        /// </summary>
        public string Host { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome de usuário para autenticação no RabbitMQ
        /// Default em ambiente de desenvolvimento: guest
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Senha para autenticação no RabbitMQ
        /// Default em ambiente de desenvolvimento: guest
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Configurações de todos os Exchanges utilizados na aplicação
        /// </summary>
        public ExchangesSettings Exchanges { get; set; } = new();
        
        /// <summary>
        /// Configurações de todas as Filas utilizadas na aplicação
        /// </summary>
        public QueuesSettings Queues { get; set; } = new();
    }

    /// <summary>
    /// Configurações dos Exchanges (pontos de distribuição de mensagens)
    /// </summary>
    public class ExchangesSettings
    {
        /// <summary>
        /// Exchange do tipo DIRECT
        /// Roteia mensagens para filas específicas baseado na routing key exata
        /// Exemplo: pedido.criado -> vai APENAS para a fila de pedidos
        /// </summary>
        public ExchangeConfig Direct { get; set; } = new();
        
        /// <summary>
        /// Exchange do tipo FANOUT
        /// Roteia mensagens para TODAS as filas vinculadas (broadcast)
        /// Exemplo: notificação geral -> vai para Email, SMS e Push simultaneamente
        /// </summary>
        public ExchangeConfig Fanout { get; set; } = new();
        
        /// <summary>
        /// Exchange do tipo TOPIC
        /// Roteia mensagens baseado em padrões de routing key (wildcards)
        /// Exemplo: log.error.* -> pega todos os erros de qualquer módulo
        ///          log.# -> pega TODOS os logs (any level)
        /// </summary>
        public ExchangeConfig Topic { get; set; } = new();
    }

    /// <summary>
    /// Configuração individual de um Exchange
    /// </summary>
    public class ExchangeConfig
    {
        /// <summary>
        /// Nome único do Exchange no RabbitMQ
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Define se o Exchange é durável (persiste após restart do RabbitMQ)
        /// true = persiste no disco, false = apenas na memória
        /// </summary>
        public bool Durable { get; set; } = true;
    }

    /// <summary>
    /// Configurações de todas as Filas da aplicação
    /// Cada fila representa um consumidor diferente
    /// </summary>
    public class QueuesSettings
    {
        // ===== DIRECT EXCHANGE QUEUES =====
        
        /// <summary>
        /// Fila para processar pedidos criados
        /// Exchange: Direct
        /// Routing Key: exata (pedido.criado)
        /// </summary>
        public QueueConfig Pedidos { get; set; } = new();
        
        // ===== FANOUT EXCHANGE QUEUES =====
        
        /// <summary>
        /// Fila para envio de notificações por email
        /// Exchange: Fanout (recebe TODAS as notificações)
        /// </summary>
        public QueueConfig Email { get; set; } = new();
        
        /// <summary>
        /// Fila para envio de notificações por SMS
        /// Exchange: Fanout (recebe TODAS as notificações)
        /// </summary>
        public QueueConfig SMS { get; set; } = new();
        
        /// <summary>
        /// Fila para envio de push notifications
        /// Exchange: Fanout (recebe TODAS as notificações)
        /// </summary>
        public QueueConfig Push { get; set; } = new();
        
        // ===== TOPIC EXCHANGE QUEUES =====
        
        /// <summary>
        /// Fila que recebe TODOS os logs (qualquer nível)
        /// Exchange: Topic
        /// Routing Pattern: log.# (# significa "qualquer coisa depois de log.")
        /// </summary>
        public QueueConfig AllLogs { get; set; } = new();
        
        /// <summary>
        /// Fila que recebe APENAS logs de erro
        /// Exchange: Topic
        /// Routing Pattern: log.error.# (apenas logs que começam com log.error)
        /// </summary>
        public QueueConfig ErrorLogs { get; set; } = new();
    }

    /// <summary>
    /// Configuração individual de uma Fila
    /// </summary>
    public class QueueConfig
    {
        /// <summary>
        /// Nome único da fila no RabbitMQ
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Routing Key para Direct Exchange (chave exata)
        /// Usado apenas em Exchanges do tipo Direct
        /// </summary>
        public string? RoutingKey { get; set; }
        
        /// <summary>
        /// Routing Pattern para Topic Exchange (padrão com wildcards)
        /// Usado apenas em Exchanges do tipo Topic
        /// Wildcards:
        ///   * = substitui exatamente UMA palavra
        ///   # = substitui ZERO ou MAIS palavras
        /// </summary>
        public string? RoutingPattern { get; set; }
    }
}
