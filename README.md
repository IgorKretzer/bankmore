# BankMore - Banco Digital

Sistema de banco digital baseado em microsserviços desenvolvido em .NET 8, seguindo os padrões DDD (Domain-Driven Design) e CQRS (Command Query Responsibility Segregation).

## Arquitetura

O sistema é composto pelos seguintes microsserviços:

- **API Conta Corrente** (Porta 5001): Gerencia contas correntes, movimentações e saldos
- **API Transferência** (Porta 5002): Gerencia transferências entre contas
- **API Tarifas** (Porta 5003): Processa tarifas via Kafka

## Funcionalidades

### API Conta Corrente
- ✅ Cadastro de contas correntes
- ✅ Autenticação via JWT
- ✅ Inativação de contas
- ✅ Movimentações (depósitos e saques)
- ✅ Consulta de saldo
- ✅ Idempotência para operações

### API Transferência
- ✅ Transferências entre contas
- ✅ Validação de saldo
- ✅ Estorno automático em caso de falha
- ✅ Comunicação entre microsserviços
- ✅ Publicação de eventos no Kafka

### API Tarifas
- ✅ Consumo de eventos de transferências via Kafka
- ✅ Aplicação automática de tarifas
- ✅ Publicação de eventos de tarifas aplicadas
- ✅ Configuração flexível do valor da tarifa

## Tecnologias Utilizadas

- **.NET 8**
- **Dapper** para acesso a dados
- **SQLite** como banco de dados
- **JWT** para autenticação
- **MediatR** para CQRS
- **Docker** para containerização
- **Kafka** para comunicação assíncrona
- **KafkaFlow** para integração com Kafka

## Como Executar

### Pré-requisitos
- Docker e Docker Compose
- .NET 8 SDK (para desenvolvimento)

### Executar com Docker Compose

1. Clone o repositório
2. Execute o comando:
```bash
docker-compose up --build
```

### Executar Localmente

1. Navegue até a pasta do projeto
2. Execute os comandos:

```bash
# API Conta Corrente
cd src/BankMore.ContaCorrente.API
dotnet run

# API Transferência (em outro terminal)
cd src/BankMore.Transferencia.API
dotnet run
```

## Endpoints da API

### API Conta Corrente (http://localhost:5001)

#### Cadastrar Conta
```http
POST /api/ContaCorrente/cadastrar
Content-Type: application/json

{
  "cpf": "12345678901",
  "nome": "João Silva",
  "senha": "123456"
}
```

#### Login
```http
POST /api/ContaCorrente/login
Content-Type: application/json

{
  "identificacao": "12345678901", // CPF ou número da conta
  "senha": "123456"
}
```

#### Consultar Saldo
```http
GET /api/ContaCorrente/saldo
Authorization: Bearer {token}
```

#### Realizar Movimentação
```http
POST /api/ContaCorrente/movimentar
Authorization: Bearer {token}
Content-Type: application/json

{
  "idRequisicao": "unique-id-123",
  "numeroConta": 1001, // Opcional - se não informado, usa a conta do token
  "valor": 100.50,
  "tipoMovimento": "C" // C = Crédito, D = Débito
}
```

#### Inativar Conta
```http
POST /api/ContaCorrente/inativar
Authorization: Bearer {token}
Content-Type: application/json

{
  "senha": "123456"
}
```

### API Transferência (http://localhost:5002)

#### Efetuar Transferência
```http
POST /api/Transferencia/efetuar
Authorization: Bearer {token}
Content-Type: application/json

{
  "idRequisicao": "unique-id-456",
  "numeroContaDestino": 1002,
  "valor": 50.00
}
```

### API Tarifas (http://localhost:5003)

A API de Tarifas funciona automaticamente via Kafka, consumindo eventos de transferências realizadas e aplicando tarifas automaticamente.

## Estrutura do Projeto

```
BankMore/
├── src/
│   ├── BankMore.Shared/                 # Classes compartilhadas
│   ├── BankMore.ContaCorrente.API/      # API Conta Corrente
│   ├── BankMore.ContaCorrente.Domain/   # Domínio Conta Corrente
│   ├── BankMore.ContaCorrente.Infrastructure/ # Infraestrutura Conta Corrente
│   ├── BankMore.Transferencia.API/      # API Transferência
│   ├── BankMore.Transferencia.Domain/   # Domínio Transferência
│   ├── BankMore.Transferencia.Infrastructure/ # Infraestrutura Transferência
│   ├── BankMore.Tarifas.API/            # API Tarifas
│   ├── BankMore.Tarifas.Domain/         # Domínio Tarifas
│   └── BankMore.Tarifas.Infrastructure/ # Infraestrutura Tarifas
├── tests/                               # Testes unitários
├── docker-compose.yml                   # Configuração Docker
└── *.sql                               # Scripts de banco de dados
```

## Padrões Implementados

### DDD (Domain-Driven Design)
- Entidades de domínio bem definidas
- Value Objects para conceitos específicos
- Repositórios para abstração de persistência
- Handlers para lógica de negócio

### CQRS (Command Query Responsibility Segregation)
- Commands para operações de escrita
- Queries para operações de leitura
- Handlers específicos para cada comando/query

### Segurança
- Autenticação JWT
- Hash de senhas com salt
- Validação de CPF
- Headers de autorização obrigatórios

### Idempotência
- Chaves de idempotência para operações críticas
- Cache para melhor performance
- Prevenção de operações duplicadas

## Testes

Execute os testes com:
```bash
dotnet test
```

## Swagger

Após executar as APIs, acesse:
- Conta Corrente: http://localhost:5001/swagger
- Transferência: http://localhost:5002/swagger
- Tarifas: http://localhost:5003/swagger

## Considerações de Produção

- Configure HTTPS em produção
- Use um banco de dados mais robusto (PostgreSQL, SQL Server)
- Configure logs estruturados
- Implemente monitoramento e métricas
- Configure backup do banco de dados
- Use secrets management para chaves JWT
