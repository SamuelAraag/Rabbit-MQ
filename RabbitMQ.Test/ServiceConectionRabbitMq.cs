using System.Security.Authentication.ExtendedProtection;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace RabbitMQ.Test;

public class ServiceConectionRabbitMq
{
    [Fact]
    public void DeveCriarConexaoValidaUsandoConnectionFactory()
    {
        //do que preciso?
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
        //do que preciso?
        var factory = new RabbitMqService();
        Assert.NotNull(factory.Connection);
        
        await using var connection = await factory.Connection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        Assert.NotNull(connection);
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
        var connection = new ConnectionFactory()
        {
            UserName =  "guest",
            Password = "guest",
            HostName = "localhost"
        };
        
        Assert.NotNull(connection);

        await using var connectionOpen = await connection.CreateConnectionAsync();
        Assert.NotNull(connectionOpen);
        Assert.True(connectionOpen.IsOpen);
    }
    
    [Fact]
    public async Task DeveDarErroAoCriarConexao_QuandoUsadoUsuarioIncorreto()
    {
        var connection = new ConnectionFactory()
        {
            UserName =  "user-incorrect",
            Password = "guest",
            HostName = "localhost"
        };
        
        Assert.NotNull(connection);

        var retorno = await Assert.ThrowsAsync<BrokerUnreachableException>(async () => await connection.CreateConnectionAsync());
        Assert.Equal("None of the specified endpoints were reachable", retorno.Message);
    }
    
    [Fact]
    public async Task DeveDarErroAoCriarConexao_QuandoUsadoSenhaIncorreta()
    {
        var connection = new ConnectionFactory()
        {
            UserName =  "guest",
            Password = "guest-incorrect",
            HostName = "localhost"
        };
        
        Assert.NotNull(connection);
        
        var retorno = await Assert.ThrowsAsync<BrokerUnreachableException>(async () => await connection.CreateConnectionAsync());
        Assert.Equal("None of the specified endpoints were reachable", retorno.Message);
    }
    
    [Fact]
    public async Task DeveDarErroAoCriarConexao_QuandoUsadoHostIncorreto()
    {
        var connection = new ConnectionFactory()
        {
            UserName =  "guest",
            Password = "guest",
            HostName = "host-dont-exist"
        };
        
        Assert.NotNull(connection);

        var retorno = await Assert.ThrowsAsync<BrokerUnreachableException>(async () => await connection.CreateConnectionAsync());
        Assert.Equal("None of the specified endpoints were reachable", retorno.Message);
    }
    
    [Fact]
    public async Task DeveCriarExchangeTopic()
    {
        var connectionFactory = new ConnectionFactory()
        {
            UserName =  "guest",
            Password = "guest",
            HostName = "localhost"
        };
        
        Assert.NotNull(connectionFactory);

        await using var connection = await connectionFactory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "exchange-teste",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }
    
    [Fact]
    public async Task DeveCriarUmaExchangeDoTipoFanout()
    {
        //do que preciso?
        var factory = new RabbitMqService();
        
        await using var connection = await factory.Connection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        //cria a exchange
        const string nomeExchange = "minha-exchange-teste";
        await channel.ExchangeDeclareAsync(
            exchange: nomeExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);
        
        //passive declare - se existe não dá erro
        //não testamos método void, testamos o colateral dele
        //Ex: o que a execucao dele causou no estado do código
        
        //pra verificar existe um metodo chamado 'ExchangeDeclarePassiveAsync' - nome da exchange
        await channel.ExchangeDeclarePassiveAsync(nomeExchange);
    }
    
    [Fact]
    public async Task DeveRetornarErroQuandoExchangeNaoExiste()
    {
        //do que preciso?
        var factory = new RabbitMqService();
        
        await using var connection = await factory.Connection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        //pra verificar existe um metodo chamado 'ExchangeDeclarePassiveAsync' - nome da exchange
        await Assert.ThrowsAsync<OperationInterruptedException>(async () => await channel.ExchangeDeclarePassiveAsync("exchange-existente"));
    }
    
    [Fact]
    public async Task DeveCriarFila()
    {
        //do que preciso?
        var factory = new RabbitMqService();
        
        await using var connection = await factory.Connection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        
        const string nomeFila = "minha-exchange-teste";
        //cria a fila
        await channel.QueueDeclareAsync(queue: nomeFila);

        //pra verificar existe um metodo chamado 'ExchangeDeclarePassiveAsync' - nome da exchange
        await channel.QueueDeclarePassiveAsync(nomeFila);
    }
    
    [Fact]
    public async Task DeveDarErroAoBuscarFilaNaoExistente()
    {
        //do que preciso?
        var factory = new RabbitMqService();
        
        await using var connection = await factory.Connection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        
        const string nomeFila = "minha-exchange-teste";
        await channel.QueueDeclareAsync(queue: nomeFila);

        //pra verificar existe um metodo chamado 'ExchangeDeclarePassiveAsync' - nome da exchange
        await Assert.ThrowsAsync<OperationInterruptedException>(async () => await channel.QueueDeclarePassiveAsync("exchange-falsa"));
    }
    
    [Fact]
    public async Task DeveFazerBindingEntreFilaComExchange()
    {
        //do que preciso?
        var factory = new RabbitMqService();
        
        await using var connection = await factory.Connection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        
        const string nomeExchange = "minha-exchange-test";
        await channel.ExchangeDeclareAsync(exchange: nomeExchange, type: ExchangeType.Fanout);
        
        const string nomeFila = "minha-exchange-teste";
        await channel.QueueDeclareAsync(queue: nomeFila);

        await channel.QueueBindAsync(nomeFila, nomeExchange, nomeFila);
        
        //como verificar se as duas estão vinculadas?
        
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
}