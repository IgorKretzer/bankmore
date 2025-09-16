# BankMore - Sistema Bancário

Olá! Este é o BankMore, um sistema bancário que desenvolvi usando microsserviços em .NET 8. A ideia era criar algo robusto mas simples de entender, seguindo algumas boas práticas como DDD e CQRS.

## Como está organizado

O projeto tem 3 APIs principais que conversam entre si:

- **Conta Corrente** (porta 5001): Aqui ficam as contas, movimentações e consultas de saldo
- **Transferência** (porta 5002): Cuida das transferências entre contas
- **Tarifas** (porta 5003): Processa as tarifas automaticamente via Kafka

## O que cada API faz

### Conta Corrente
Aqui é onde tudo começa - cadastro de contas, login, movimentações básicas. Implementei autenticação JWT e um sistema de idempotência para evitar operações duplicadas (já tive problema com isso antes 😅).

### Transferência
Esta API cuida das transferências entre contas. O legal é que ela valida se tem saldo suficiente e, se algo der errado, faz o estorno automaticamente. Também publica eventos no Kafka para que outras APIs saibam quando uma transferência aconteceu.

### Tarifas
Esta API fica "escutando" os eventos de transferência e aplica as tarifas automaticamente. Ainda está bem simples, mas a estrutura está pronta para crescer.

## Stack que usei

- **.NET 8** - Framework principal
- **Dapper** - ORM leve para acesso aos dados (não gosto de Entity Framework para tudo)
- **SQLite** - Banco simples para desenvolvimento (em produção usaria PostgreSQL)
- **JWT** - Autenticação stateless
- **MediatR** - Para implementar o padrão CQRS de forma limpa
- **Docker** - Para facilitar o deploy
- **Kafka** - Para comunicação assíncrona entre os microsserviços
- **KafkaFlow** - Biblioteca que facilita a integração com Kafka

## Como rodar o projeto

### O que você precisa
- Docker e Docker Compose instalados
- .NET 8 SDK (se quiser rodar localmente)

### Opção 1: Docker (mais fácil)
```bash
# Clone o repo e rode:
docker-compose up --build
```

### Opção 2: Local (para debug)
Se quiser rodar localmente para debugar, precisa subir cada API em um terminal diferente:

```bash
# Terminal 1 - Conta Corrente
cd src/BankMore.ContaCorrente.API
ASPNETCORE_URLS=http://localhost:5001 dotnet run

# Terminal 2 - Transferência  
cd src/BankMore.Transferencia.API
ASPNETCORE_URLS=http://localhost:5002 dotnet run

# Terminal 3 - Tarifas
cd src/BankMore.Tarifas.API
ASPNETCORE_URLS=http://localhost:5003 dotnet run
```

## Testando as APIs

### Conta Corrente (http://localhost:5001)

**Cadastrar uma conta:**
```bash
curl -X POST http://localhost:5001/api/ContaCorrente/cadastrar \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "nome": "João Silva", 
    "senha": "123456"
  }'
```

**Fazer login:**
```bash
curl -X POST http://localhost:5001/api/ContaCorrente/login \
  -H "Content-Type: application/json" \
  -d '{
    "identificacao": "12345678901",
    "senha": "123456"
  }'
```

**Consultar saldo (precisa do token do login):**
```bash
curl -X GET http://localhost:5001/api/ContaCorrente/saldo \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

**Fazer um depósito:**
```bash
curl -X POST http://localhost:5001/api/ContaCorrente/movimentar \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "idRequisicao": "dep-001",
    "valor": 100.50,
    "tipoMovimento": "C"
  }'
```

### Transferência (http://localhost:5002)

**Transferir dinheiro:**
```bash
curl -X POST http://localhost:5002/api/Transferencia/efetuar \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{
    "idRequisicao": "trans-001",
    "numeroContaDestino": 2,
    "valor": 50.00
  }'
```

### Tarifas (http://localhost:5003)

A API de tarifas funciona sozinha - ela "escuta" as transferências e aplica as tarifas automaticamente. Você pode consultar as tarifas aplicadas:

```bash
curl -X GET http://localhost:5003/api/Tarifas/consultar
```

## Como o código está organizado

```
BankMore/
├── src/
│   ├── BankMore.Shared/                 # Classes que uso em todos os projetos
│   ├── BankMore.ContaCorrente.API/      # API principal
│   ├── BankMore.ContaCorrente.Domain/   # Regras de negócio
│   ├── BankMore.ContaCorrente.Infrastructure/ # Acesso a dados
│   ├── BankMore.Transferencia.API/      # API de transferências
│   ├── BankMore.Transferencia.Domain/   # Lógica de transferências
│   ├── BankMore.Transferencia.Infrastructure/ # Integração com outras APIs
│   ├── BankMore.Tarifas.API/            # API de tarifas
│   ├── BankMore.Tarifas.Domain/         # Regras de tarifas
│   └── BankMore.Tarifas.Infrastructure/ # Kafka e persistência
├── tests/                               # Testes (só alguns por enquanto)
├── docker-compose.yml                   # Para subir tudo junto
└── *.sql                               # Scripts para criar as tabelas
```

## Algumas decisões que tomei

### DDD (Domain-Driven Design)
Tentei separar bem as responsabilidades - cada domínio tem suas próprias regras e não depende de outros. Isso facilita manutenção e testes.

### CQRS 
Separei comandos (que modificam dados) de queries (que só leem). O MediatR ajuda muito nisso.

### Segurança
- JWT para autenticação (sem sessão no servidor)
- Senhas com hash + salt (nunca salvo em texto)
- Validação de CPF (básica, mas funciona)
- Todos os endpoints sensíveis precisam de token

### Idempotência
Implementei um sistema para evitar operações duplicadas - cada operação tem um ID único. Se você tentar fazer a mesma operação duas vezes, só executa uma vez.

## Testes

Para rodar os testes:
```bash
dotnet test
```

Confesso que não cobri tudo com testes ainda - foquei mais na estrutura. Em um projeto real, testaria mais a fundo.

## Swagger

Cada API tem sua documentação automática:
- Conta Corrente: http://localhost:5001/swagger
- Transferência: http://localhost:5002/swagger  
- Tarifas: http://localhost:5003/swagger

## O que mudaria em produção

- **HTTPS** obrigatório (aqui está HTTP para facilitar testes)
- **PostgreSQL** ou **SQL Server** no lugar do SQLite
- **Redis** para cache distribuído
- **Logs estruturados** com Serilog
- **Monitoramento** com Application Insights ou similar
- **Secrets** para chaves JWT (não hardcoded)
- **Rate limiting** nos endpoints
- **Health checks** para monitorar as APIs

