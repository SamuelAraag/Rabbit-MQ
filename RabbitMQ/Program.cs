using RabbitMQ.Controllers;
using RabbitMQ.Extensions;
using RabbitMQ.Models;

/// <summary>
/// PROJETO DE ESTUDO: RabbitMQ com .NET 8
/// 
/// OBJETIVO:
/// Demonstrar na prática os três principais tipos de Exchange:
/// 1. DIRECT Exchange - Roteamento com routing key exata
/// 2. FANOUT Exchange - Broadcast para todos os consumers
/// 3. TOPIC Exchange - Pattern matching com wildcards
/// 
/// TECNOLOGIAS:
/// - .NET 8
/// - MassTransit (abstração do RabbitMQ)
/// - RabbitMQ (mensageria)
/// - Docker (para rodar o RabbitMQ)
/// 
/// COMO EXECUTAR:
/// 1. Certifique-se que o Docker está rodando
/// 2. Execute: docker-compose up -d
/// 3. Acesse: http://localhost:15672 (guest/guest)
/// 4. Execute a aplicação: dotnet run
/// 5. Acesse: http://localhost:5000/swagger
/// 6. Teste os endpoints!
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAÇÃO DE SERVIÇOS =====

// Controllers para os endpoints da API
builder.Services.AddControllers();

// Swagger para documentação e testes da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== CONFIGURAÇÃO DO RABBITMQ =====
// Lê as configurações do appsettings.json e vincula à classe RabbitMQSettings
var rabbitMQSettings = builder.Configuration
    .GetSection("RabbitMQ")
    .Get<RabbitMQSettings>();

// Validação básica da configuração
if (rabbitMQSettings == null)
{
    throw new InvalidOperationException(
        "Configuração do RabbitMQ não encontrada no appsettings.json! " +
        "Verifique se a seção 'RabbitMQ' está configurada corretamente."
    );
}

// Registra o serviço do RabbitMQ com MassTransit
// Este método está definido em Extensions/AppExtension.cs
builder.Services.AddRabbitMQService(rabbitMQSettings);

// ===== CONFIGURAÇÃO DA APLICAÇÃO =====

var app = builder.Build();

// Adiciona os endpoints da API definidos em Controllers/ApiEndpoints.cs
app.AddApiEndpoint();

// Swagger apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ===== MENSAGEM DE INÍCIO =====
Console.WriteLine("====================================");
Console.WriteLine("RabbitMQ Study Project");
Console.WriteLine("====================================");
Console.WriteLine($"RabbitMQ Host: {rabbitMQSettings.Host}");
Console.WriteLine($"Username: {rabbitMQSettings.Username}");
Console.WriteLine("====================================");
Console.WriteLine("Exchanges configurados:");
Console.WriteLine($"  • DIRECT:  {rabbitMQSettings.Exchanges.Direct.Name}");
Console.WriteLine($"  • FANOUT:  {rabbitMQSettings.Exchanges.Fanout.Name}");
Console.WriteLine($"  • TOPIC:   {rabbitMQSettings.Exchanges.Topic.Name}");
Console.WriteLine("====================================");
Console.WriteLine("Endpoints disponíveis:");
Console.WriteLine("  • POST /api/pedido       (Direct Exchange)");
Console.WriteLine("  • POST /api/notificacao  (Fanout Exchange)");
Console.WriteLine("  • POST /api/log          (Topic Exchange)");
Console.WriteLine("  • GET  /api/status       (Status do sistema)");
Console.WriteLine("====================================");
Console.WriteLine("Acesse o Swagger para testar:");
Console.WriteLine("  http://localhost:5000/swagger");
Console.WriteLine("====================================");
Console.WriteLine("Interface do RabbitMQ:");
Console.WriteLine("  http://localhost:15672 (guest/guest)");
Console.WriteLine("====================================");

app.Run();
