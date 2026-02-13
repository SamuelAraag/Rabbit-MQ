# Projeto de Estudo: RabbitMQ com .NET 8

Projeto didático completo para aprender **RabbitMQ** com exemplos práticos dos três principais tipos de Exchange: **Direct**, **Fanout** e **Topic**.

---

## O que você vai aprender

Este projeto demonstra:

### 1. **Direct Exchange** - Roteamento Exato
- Mensagem vai para UMA fila específica baseada na routing key **exata**
- **Exemplo**: Pedido criado → somente o serviço de processamento de pedidos recebe

### 2. **Fanout Exchange** - Broadcast
- Mensagem é **copiada** para TODAS as filas vinculadas
- **Exemplo**: Notificação importante → vai para Email, SMS e Push **simultaneamente**

### 3. **Topic Exchange** - Pattern Matching
- Mensagem é roteada baseada em **padrões** com wildcards (`*` e `#`)
- **Exemplo**: Logs filtrados por nível e módulo

---

## Como Executar

### Pré-requisitos

- **.NET 8 SDK** instalado
- **Docker** rodando
- **Terminal** ou **VS Code**

### Passo a Passo

#### 1. Subir o RabbitMQ com Docker

```bash
cd /Users/samuelaraag/Downloads/documentos/projetos/RabbitMQ
docker-compose up -d
```

Isso vai:
- Baixar a imagem do RabbitMQ (se necessário)
- Subir o container
- Expor as portas 5672 (AMQP) e 15672 (Interface Web)

#### 2. Verificar se o RabbitMQ está rodando

Acesse: **http://localhost:15672**
- **Usuário**: `guest`
- **Senha**: `guest`

#### 3. Executar a aplicação

```bash
dotnet run --project RabbitMQ
```

Você verá algo como:

```
====================================
RabbitMQ Study Project
====================================
RabbitMQ Host: amqp://localhost:5672
Username: guest
====================================
Exchanges configurados:
  • DIRECT:  marketplace.direct
  • FANOUT:  notifications.fanout
  • TOPIC:   logs.topic
====================================
Endpoints disponíveis:
  • POST /api/pedido       (Direct Exchange)
  • POST /api/notificacao  (Fanout Exchange)
  • POST /api/log          (Topic Exchange)
  • GET  /api/status       (Status do sistema)
====================================
```

#### 4. Testar no Swagger

Acesse: **http://localhost:5000/swagger**

---

## Testando os Endpoints

### 1. Direct Exchange - Criar Pedido

**Endpoint**: `POST /api/pedido`

**Body**:
```json
{
  "nomeCliente": "João Silva",
  "valorTotal": 299.90
}
```

**O que acontece**:
1. Event `PedidoCriadoEvent` é criado
2. Publicado no Exchange `marketplace.direct`
3. Roteado para fila `pedidos.queue` (routing key: `pedido.criado`)
4. `PedidoCriadoConsumer` recebe e processa

**Verifique os logs no console** - você verá:
```
[DIRECT EXCHANGE] Pedido recebido - CorrelationId: {guid}, Cliente: João Silva, Valor: R$ 299,90
[DIRECT EXCHANGE] Pedido processado com sucesso - CorrelationId: {guid}
```

---

### 2. Fanout Exchange - Enviar Notificação

**Endpoint**: `POST /api/notificacao`

**Body**:
```json
{
  "titulo": "Promoção Relâmpago!",
  "mensagem": "50% de desconto em todos os produtos",
  "destinatario": "Maria Santos"
}
```

**O que acontece**:
1. Event `NotificacaoGeralEvent` é criado
2. Publicado no Exchange `notifications.fanout`
3. **COPIADO** para 3 filas:
   - `notificacao.email.queue` → EmailConsumer
   - `notificacao.sms.queue` → SMSConsumer
   - `notificacao.push.queue` → PushNotificationConsumer
4. Os **3 consumers processam SIMULTANEAMENTE**

**Verifique os logs no console** - você verá **3 mensagens**:
```
[FANOUT - EMAIL] Enviando email - CorrelationId: {guid}, Para: Maria Santos
[FANOUT - SMS] Enviando SMS - CorrelationId: {guid}, Para: Maria Santos
[FANOUT - PUSH] Enviando push notification - CorrelationId: {guid}, Para: Maria Santos
```

---

### 3. Topic Exchange - Registrar Log

**Endpoint**: `POST /api/log`

#### Teste 1: Log de INFO (vai apenas para AllLogsConsumer)

**Body**:
```json
{
  "nivel": "info",
  "modulo": "pedidos",
  "mensagem": "Pedido criado com sucesso"
}
```

**O que acontece**:
- Routing key gerada: `log.info.pedidos`
- `AllLogsConsumer` recebe (pattern: `log.#` = **tudo**)
- `ErrorLogsConsumer` **NÃO** recebe (pattern: `log.error.#` = apenas erros)

**Logs**:
```
[TOPIC - ALL LOGS] Log capturado - Nível: info, Módulo: pedidos
```

#### Teste 2: Log de ERROR (vai para AMBOS os consumers)

**Body**:
```json
{
  "nivel": "error",
  "modulo": "pagamentos",
  "mensagem": "Falha ao processar pagamento"
}
```

**O que acontece**:
- Routing key gerada: `log.error.pagamentos`
- `AllLogsConsumer` recebe (pattern: `log.#`)
- `ErrorLogsConsumer` **TAMBÉM** recebe (pattern: `log.error.#`)

**Logs**:
```
[TOPIC - ALL LOGS] Log capturado - Nível: error, Módulo: pagamentos
[TOPIC - ERROR LOGS] ERRO DETECTADO - Módulo: pagamentos
```

#### Teste 3: Log de WARNING (vai apenas para AllLogsConsumer)

**Body**:
```json
{
  "nivel": "warning",
  "modulo": "usuarios",
  "mensagem": "Tentativa de login suspeita"
}
```

---

## Estrutura do Projeto

```
RabbitMQ/
├── Models/
│   └── RabbitMQSettings.cs          # Configurações tipadas do appsettings.json
├── Events/
│   ├── Direct/
│   │   └── PedidoCriadoEvent.cs     # Event para Direct Exchange
│   ├── Fanout/
│   │   └── NotificacaoGeralEvent.cs # Event para Fanout Exchange
│   └── Topic/
│       └── LogEvent.cs              # Event para Topic Exchange
├── Consumers/
│   ├── Direct/
│   │   └── PedidoCriadoConsumer.cs      # Processa pedidos
│   ├── Fanout/
│   │   ├── EmailConsumer.cs             # Envia email
│   │   ├── SMSConsumer.cs               # Envia SMS
│   │   └── PushNotificationConsumer.cs  # Envia push
│   └── Topic/
│       ├── AllLogsConsumer.cs       # Recebe TODOS os logs
│       └── ErrorLogsConsumer.cs     # Recebe APENAS erros
├── Extensions/
│   └── AppExtension.cs              # Configuração do MassTransit
├── Controllers/
│   └── ApiEndpoints.cs              # Endpoints da API
├── Program.cs                       # Entry point
├── appsettings.json                 # Configurações externalizadas
└── docker-compose.yml               # Docker do RabbitMQ
```

---

## Conceitos Importantes

### Correlation ID
- Identificador único para rastrear uma mensagem em toda a aplicação
- Útil para debugging e tracking de erros
- Você verá o mesmo ID nos logs do producer e do consumer

### Wildcards no Topic Exchange

| Wildcard | Descrição | Exemplo |
|----------|-----------|---------|
| `*` | Substitui **exatamente UMA** palavra | `log.*.pedidos` recebe `log.info.pedidos` e `log.error.pedidos` |
| `#` | Substitui **ZERO ou MAIS** palavras | `log.#` recebe `log.info`, `log.error.pedidos`, `log.warning.api.gateway` |

### Publish/Subscribe Pattern
- **Producer** (quem publica) não conhece o **Consumer** (quem consome)
- Desacoplamento total
- Fácil adicionar novos consumers sem modificar o producer

---

## Monitorando no RabbitMQ

1. Acesse: **http://localhost:15672**
2. Vá em **Exchanges** → Você verá:
   - `marketplace.direct` (tipo: direct)
   - `notifications.fanout` (tipo: fanout)
   - `logs.topic` (tipo: topic)
3. Vá em **Queues** → Você verá:
   - `pedidos.queue`
   - `notificacao.email.queue`, `notificacao.sms.queue`, `notificacao.push.queue`
   - `logs.all.queue`, `logs.error.queue`
4. Vá em **Connections** → Você verá a conexão ativa da aplicação

---

## Dicas de Estudo

1. **Teste um endpoint por vez** e observe os logs
2. **Acompanhe no RabbitMQ** a criação das exchanges e filas
3. **Leia os comentários no código** - eles explicam cada conceito
4. **Experimente**:
   - Enviar múltiplos pedidos
   - Enviar notificações e ver os 3 consumers rodando
   - Alternar entre logs de diferentes níveis

---

## Comandos Úteis

```bash
# Subir o RabbitMQ
docker-compose up -d

# Ver logs do RabbitMQ
docker logs rabbitmq -f

# Parar o RabbitMQ
docker-compose down

# Executar a aplicação
dotnet run --project RabbitMQ

# Compilar
dotnet build
```

---

## Próximos Passos

Após dominar estes conceitos, você pode:

1. **Headers Exchange** - Roteamento baseado em headers HTTP
2. **Dead Letter Queue** - Tratamento de mensagens com falha
3. **Retry Policy** - Configurar retentativas automáticas
4. **Message TTL** - Tempo de vida das mensagens
5. **Persistent Messages** - Mensagens que sobrevivem a restart

---

## Contribuições

Este é um projeto de estudo. Sinta-se livre para:
- Adicionar novos exemplos
- Melhorar a documentação
- Criar testes unitários
- Compartilhar seu aprendizado

---

**Bons estudos!**