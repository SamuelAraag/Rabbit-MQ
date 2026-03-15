using RabbitMQ.Client;

namespace RabbitMQ.Test.Integration;

public abstract class RabbitMqIntegrationTestBase : IAsyncLifetime
{
    // conexao compartilhada para o teste
    protected IConnection Connection { get; private set; } = null!;

    // canal exclusivo do teste, criado no InitializeAsync e destruido no DisposeAsync
    protected IChannel Channel { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        try
        {
            var factory = new ConnectionFactory 
            { 
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            Connection = await factory.CreateConnectionAsync();
            Channel = await Connection.CreateChannelAsync();
        
            await ConfigurarAsync(Channel);
        }
        catch (Exception ex)
        {
            throw new Exception("Error to initialize Rabbit MQ. Certify if Server is running in Docker.", ex);
        }
    }

    // ponto de extensao para as subclasses declararem exchanges, filas e bindings
    protected virtual Task ConfigurarAsync(IChannel channel) => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (Channel != null)
        {
            await Channel.DisposeAsync();
        }

        if (Connection != null)
        {
            await Connection.DisposeAsync();
        }
    }
}
