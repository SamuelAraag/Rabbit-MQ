namespace RabbitMQ.Events.Topic
{
    /// <summary>
    /// Event que representa um log da aplicação
    /// 
    /// CONCEITO: TOPIC EXCHANGE
    /// - Este evento será enviado para um Exchange do tipo TOPIC
    /// - No Topic Exchange, a mensagem é roteada baseada em PADRÕES de routing key (wildcards)
    /// - Usa wildcards: * (substitui UMA palavra) e # (substitui ZERO ou MAIS palavras)
    /// 
    /// QUANDO USAR TOPIC EXCHANGE?
    /// - Quando você quer roteamento flexível baseado em padrões
    /// - Exemplo: Sistema de logs onde você quer filtrar por nível e módulo
    /// 
    /// PADRÃO DE ROUTING KEY NESTE EXEMPLO:
    /// - Formato: "log.{nivel}.{modulo}"
    /// - Exemplos:
    ///   * "log.info.pedidos"     -> log de informação do módulo de pedidos
    ///   * "log.error.pagamentos" -> log de erro do módulo de pagamentos
    ///   * "log.warning.usuarios" -> log de aviso do módulo de usuários
    /// 
    /// PADRÕES DE CONSUMO:
    /// - "log.#"         -> recebe TODOS os logs (qualquer nível, qualquer módulo)
    /// - "log.error.#"   -> recebe APENAS logs de erro (de qualquer módulo)
    /// - "log.*.pedidos" -> recebe logs de QUALQUER nível do módulo pedidos
    /// </summary>
    /// <param name="Id">ID único do log (Correlation ID)</param>
    /// <param name="Nivel">Nível do log: info, warning, error</param>
    /// <param name="Modulo">Módulo que gerou o log: pedidos, pagamentos, usuarios, etc</param>
    /// <param name="Mensagem">Mensagem descritiva do log</param>
    /// <param name="DataHora">Data e hora da ocorrência</param>
    public record LogEvent(
        Guid Id,
        string Nivel,      // info, warning, error
        string Modulo,     // pedidos, pagamentos, usuarios, etc
        string Mensagem,
        DateTime DataHora
    )
    {
        /// <summary>
        /// Gera a routing key no padrão: log.{nivel}.{modulo}
        /// Esta routing key será usada para rotear a mensagem no Topic Exchange
        /// </summary>
        public string GetRoutingKey() => $"log.{Nivel}.{Modulo}";
    }
}
