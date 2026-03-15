using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Test.Integration;

namespace RabbitMQ.Test;

public class ServiceConectionRabbitMq : RabbitMqIntegrationTestBase
{
    [Fact]
    public void DeveCriarConexaoValidaUsandoConnectionFactory()
    {
        const string hostname = "localhost";
        const string username = "guest";
        const string password = "guest";

        var rabbitService = new RabbitMqService();

        Assert.NotNull(rabbitService);
        Assert.Equal(hostname, rabbitService.Connection.HostName);
        Assert.Equal(username, rabbitService.Connection.UserName);
        Assert.Equal(password, rabbitService.Connection.Password);
    }

    [Fact]
    public async Task DeveCriarUmaConnectionAsynComChannelValida()
    {
        Assert.NotNull(Connection);

        await using var channel = await Connection.CreateChannelAsync();

        Assert.NotNull(Connection);
        Assert.NotNull(channel);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    public void DeveDeclararUmaFuncaoSomar(int firstNumber, int secondNumber, int expectedResult)
    {
        //DOC: como declarar uma fariavel com uma funcao,
        //tipo Função = Func<>
        //tipo param 1, tipo param 2, tipo retorno = <int, int, int>
        //nome da funcao = Somar
        //Arrow function, no caso com parametros = (a, b) =>
        //processo da funcao = a+b;
        Func<int, int, int> sum = (a, b) => a + b;

        var result = sum(firstNumber, secondNumber);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void MustReturnRangeNumber()
    {
        //DOC: como usar o random
        //instancia o Random
        //use o metodo Next = .Next(1, 11);
        //Importante: primeiro parametro é igual ou maior : >= 1
        //Segundo parametro é menor que < = 11, no caso ele pegaria de 1 até 10
        Func<int> RetornaUmNumeroAleatorio = () => new Random().Next(1, 101);

        var result = RetornaUmNumeroAleatorio();

        var ehValorEsperado = result > 0 && result < 100;
        Assert.True(ehValorEsperado);
    }

    [Fact]
    public async Task DeveCriarConexao_QuandoUsadoConfiguracoesDefault()
    {
        Assert.NotNull(Connection);
        Assert.True(Connection.IsOpen);
    }

    [Fact]
    public async Task DeveDarErroAoCriarConexao_QuandoUsadoUsuarioIncorreto()
    {
        var factory = new ConnectionFactory
        {
            UserName = "user-incorrect",
            Password = "guest",
            HostName = "localhost"
        };

        var retorno = await Assert.ThrowsAsync<BrokerUnreachableException>(
            async () => await factory.CreateConnectionAsync()
        );
        Assert.Equal("None of the specified endpoints were reachable", retorno.Message);
    }

    [Fact]
    public async Task DeveDarErroAoCriarConexao_QuandoUsadoSenhaIncorreta()
    {
        var factory = new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest-incorrect",
            HostName = "localhost"
        };

        var retorno = await Assert.ThrowsAsync<BrokerUnreachableException>(
            async () => await factory.CreateConnectionAsync()
        );
        Assert.Equal("None of the specified endpoints were reachable", retorno.Message);
    }

    [Fact]
    public async Task DeveDarErroAoCriarConexao_QuandoUsadoHostIncorreto()
    {
        var factory = new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest",
            HostName = "host-dont-exist"
        };

        var retorno = await Assert.ThrowsAsync<BrokerUnreachableException>(
            async () => await factory.CreateConnectionAsync()
        );
        Assert.Equal("None of the specified endpoints were reachable", retorno.Message);
    }

    [Fact]
    public async Task DeveCriarExchangeTopic()
    {
        await Channel.ExchangeDeclareAsync(
            exchange: "exchange-teste",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        await Channel.ExchangeDeclarePassiveAsync("exchange-teste");
    }

    [Fact]
    public async Task DeveCriarUmaExchangeDoTipoFanout()
    {
        const string nomeExchange = "minha-exchange-fanout";
        await Channel.ExchangeDeclareAsync(
            exchange: nomeExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false
        );

        //passive declare - se existe não dá erro
        //não testamos método void, testamos o colateral dele
        //Ex: o que a execucao dele causou no estado do código
        
        //pra verificar existe um metodo chamado 'ExchangeDeclarePassiveAsync' - nome da exchange
        await Channel.ExchangeDeclarePassiveAsync(nomeExchange);
    }

    [Fact]
    public async Task DeveRetornarErroQuandoExchangeNaoExiste()
    {
        await using var channel = await Connection.CreateChannelAsync();

        //pra verificar existe um metodo chamado 'ExchangeDeclarePassiveAsync' - nome da exchange
        await Assert.ThrowsAsync<OperationInterruptedException>(async () => await channel.ExchangeDeclarePassiveAsync("exchange-existente"));
    }

    [Fact]
    public async Task DeveCriarFila()
    {
        const string nomeFila = "fila-teste";
        await Channel.QueueDeclareAsync(queue: nomeFila);

        await Channel.QueueDeclarePassiveAsync(nomeFila);
    }

    [Fact]
    public async Task DeveDarErroAoBuscarFilaNaoExistente()
    {
        await using var channel = await Connection.CreateChannelAsync();

        await Assert.ThrowsAsync<OperationInterruptedException>(
            async () => await channel.QueueDeclarePassiveAsync("fila-que-nao-existe")
        );
    }

    [Fact]
    public async Task DeveFazerBindingEntreFilaComExchange()
    {
        const string nomeExchange = "exchange-binding-test";
        await Channel.ExchangeDeclareAsync(exchange: nomeExchange, type: ExchangeType.Fanout);

        const string nomeFila = "fila-binding-test";
        await Channel.QueueDeclareAsync(queue: nomeFila);

        const string routingKey = "routing-key-test";
        await Channel.QueueBindAsync(nomeFila, nomeExchange, routingKey);

        //como verificar se as duas estão vinculadas?
        //publica a mensagem
        //consome pra ver se chegou na fila
        
        //producer está dentro do channel
        const string mensagem = "mensagem enviada ao exchang";
        var body = Encoding.UTF8.GetBytes(mensagem);
        await Channel.BasicPublishAsync(
            exchange: nomeExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body
        );

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += async (model, basicEventArgs) =>
        {
            var bodyRecebido = basicEventArgs.Body.ToArray();
            var messageRecebida = Encoding.UTF8.GetString(bodyRecebido);
            Assert.Equal(mensagem, messageRecebida);
        };

        var consumerTag = await Channel.BasicConsumeAsync(
            nomeFila,
            autoAck: true,
            consumer: consumer
        );
    }
    
    // estes de Queue (Muito Importante)
    // Deve criar fila
    // Deve fazer bind entre queue e exchange
    // Deve falhar ao publicar sem binding
    //
    //Esses testes ajudam você a entender roteamento.
    
    //Criar exchange
    // Criar queue
    // Bind
    // Publicar mensagem
    // Consumir manualmente para validar
    
    //Deve conectar ao RabbitMQ
    // 
    // Deve falhar com host inválido
    // 
    // Deve falhar com credenciais inválidas
    // 
    // Deve declarar exchange
    // 
    // Deve declarar queue
    // 
    // Deve fazer bind entre exchange e queue
    // 
    // Deve publicar mensagem
    // 
    // Deve consumir mensagem
    // 
    // Deve fazer ACK manual
    // 
    // Deve reenfileirar mensagem sem ACK
    // 
    // Deve publicar mensagem persistente
    // 
    // Deve configurar fila como durable
    // 
    // Deve enviar mensagem para Dead Letter Queue
    // 
    // Deve aplicar TTL na mensagem
    // 
    // Deve aplicar TTL na fila
    // 
    // Deve respeitar prefetch (BasicQos)
    // 
    // Deve processar múltiplas mensagens concorrentes
    // 
    // Deve lidar com retry após falha
    // 
    // Deve lidar com exception no consumer
    // 
    // Deve mockar BasicPublish em teste unitário
    //estudar o uso do token e canceleation token
}