using RabbitMQ.Client;

namespace RabbitMQ;

public class RabbitMqService
{
    private readonly ConnectionFactory _connection;
    
    private readonly string _hostname = "localhost";
    private readonly string _username = "guest";
    private readonly string _password = "guest";
    
    public RabbitMqService()
    {
        _connection = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName =  _username,
            Password = _password,
        };
    }

    public ConnectionFactory Connection => _connection;
}