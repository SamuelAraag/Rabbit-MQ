namespace RabbitMQ.Events.Fanout
{
    /// <summary>
    /// Event que representa uma notificação geral que deve ser enviada por todos os canais
    /// 
    /// CONCEITO: FANOUT EXCHANGE
    /// - Este evento será enviado para um Exchange do tipo FANOUT
    /// - No Fanout Exchange, a mensagem é COPIADA para TODAS as filas vinculadas (broadcast)
    /// - NÃO usa routing key - simplesmente distribui para todos
    /// 
    /// QUANDO USAR FANOUT EXCHANGE?
    /// - Quando você quer que TODOS os consumidores recebam a mesma mensagem
    /// - Exemplo: Notificação importante -> deve ir para Email, SMS e Push simultaneamente
    /// - É o padrão Publish/Subscribe clássico
    /// 
    /// NESTE EXEMPLO:
    /// - Uma notificação será enviada
    /// - Três consumers irão receber: EmailConsumer, SMSConsumer e PushNotificationConsumer
    /// - Todos processarão a mesma mensagem de forma independente
    /// </summary>
    /// <param name="Id">ID único da notificação (Correlation ID)</param>
    /// <param name="Titulo">Título da notificação</param>
    /// <param name="Mensagem">Conteúdo da mensagem</param>
    /// <param name="Destinatario">Nome ou identificador do destinatário</param>
    /// <param name="DataEnvio">Data e hora do envio</param>
    public record NotificacaoGeralEvent(
        Guid Id,
        string Titulo,
        string Mensagem,
        string Destinatario,
        DateTime DataEnvio
    );
}
