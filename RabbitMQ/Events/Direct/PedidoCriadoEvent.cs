namespace RabbitMQ.Events.Direct
{
    /// <summary>
    /// Event que representa a criação de um pedido
    /// 
    /// CONCEITO: DIRECT EXCHANGE
    /// - Este evento será enviado para um Exchange do tipo DIRECT
    /// - No Direct Exchange, a mensagem vai para UMA fila específica baseada na routing key EXATA
    /// - Routing Key: "pedido.criado" -> somente a fila que estiver vinculada com essa chave receberá
    /// 
    /// QUANDO USAR DIRECT EXCHANGE?
    /// - Quando você quer que a mensagem vá para um destino específico
    /// - Exemplo: Pedido criado -> somente o serviço de processamento de pedidos deve receber
    /// </summary>
    /// <param name="Id">ID único do pedido (Correlation ID para tracking)</param>
    /// <param name="NomeCliente">Nome do cliente que fez o pedido</param>
    /// <param name="ValorTotal">Valor total do pedido</param>
    /// <param name="DataCriacao">Data e hora da criação do pedido</param>
    public record PedidoCriadoEvent(
        Guid Id,
        string NomeCliente,
        decimal ValorTotal,
        DateTime DataCriacao
    );
}
