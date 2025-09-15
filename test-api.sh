#!/bin/bash

echo "=== Testando API BankMore ==="
echo

# Configurações
CONTA_API="http://localhost:5001"
TRANSFERENCIA_API="http://localhost:5002"
TARIFAS_API="http://localhost:5003"

echo "1. Cadastrando primeira conta..."
CONTA1_RESPONSE=$(curl -s -X POST "$CONTA_API/api/ContaCorrente/cadastrar" \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "nome": "João Silva",
    "senha": "123456"
  }')

echo "Resposta: $CONTA1_RESPONSE"
CONTA1_NUMERO=$(echo $CONTA1_RESPONSE | grep -o '"numeroConta":[0-9]*' | grep -o '[0-9]*')
echo "Número da conta: $CONTA1_NUMERO"
echo

echo "2. Cadastrando segunda conta..."
CONTA2_RESPONSE=$(curl -s -X POST "$CONTA_API/api/ContaCorrente/cadastrar" \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "98765432100",
    "nome": "Maria Santos",
    "senha": "654321"
  }')

echo "Resposta: $CONTA2_RESPONSE"
CONTA2_NUMERO=$(echo $CONTA2_RESPONSE | grep -o '"numeroConta":[0-9]*' | grep -o '[0-9]*')
echo "Número da conta: $CONTA2_NUMERO"
echo

echo "3. Fazendo login na primeira conta..."
LOGIN1_RESPONSE=$(curl -s -X POST "$CONTA_API/api/ContaCorrente/login" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacao": "'$CONTA1_NUMERO'",
    "senha": "123456"
  }')

echo "Resposta: $LOGIN1_RESPONSE"
TOKEN1=$(echo $LOGIN1_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Token: $TOKEN1"
echo

echo "4. Fazendo depósito na primeira conta..."
DEPOSITO_RESPONSE=$(curl -s -X POST "$CONTA_API/api/ContaCorrente/movimentar" \
  -H "Authorization: Bearer $TOKEN1" \
  -H "Content-Type: application/json" \
  -d '{
    "idRequisicao": "dep-001",
    "valor": 1000.00,
    "tipoMovimento": "C"
  }')

echo "Resposta: $DEPOSITO_RESPONSE"
echo

echo "5. Consultando saldo da primeira conta..."
SALDO_RESPONSE=$(curl -s -X GET "$CONTA_API/api/ContaCorrente/saldo" \
  -H "Authorization: Bearer $TOKEN1")

echo "Resposta: $SALDO_RESPONSE"
echo

echo "6. Fazendo login na segunda conta..."
LOGIN2_RESPONSE=$(curl -s -X POST "$CONTA_API/api/ContaCorrente/login" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacao": "'$CONTA2_NUMERO'",
    "senha": "654321"
  }')

echo "Resposta: $LOGIN2_RESPONSE"
TOKEN2=$(echo $LOGIN2_RESPONSE | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Token: $TOKEN2"
echo

echo "7. Fazendo depósito na segunda conta..."
DEPOSITO2_RESPONSE=$(curl -s -X POST "$CONTA_API/api/ContaCorrente/movimentar" \
  -H "Authorization: Bearer $TOKEN2" \
  -H "Content-Type: application/json" \
  -d '{
    "idRequisicao": "dep-002",
    "valor": 500.00,
    "tipoMovimento": "C"
  }')

echo "Resposta: $DEPOSITO2_RESPONSE"
echo

echo "8. Fazendo transferência da primeira para a segunda conta..."
TRANSFERENCIA_RESPONSE=$(curl -s -X POST "$TRANSFERENCIA_API/api/Transferencia/efetuar" \
  -H "Authorization: Bearer $TOKEN1" \
  -H "Content-Type: application/json" \
  -d '{
    "idRequisicao": "trans-001",
    "numeroContaDestino": '$CONTA2_NUMERO',
    "valor": 200.00
  }')

echo "Resposta: $TRANSFERENCIA_RESPONSE"
echo

echo "9. Consultando saldo da primeira conta após transferência..."
SALDO1_FINAL=$(curl -s -X GET "$CONTA_API/api/ContaCorrente/saldo" \
  -H "Authorization: Bearer $TOKEN1")

echo "Resposta: $SALDO1_FINAL"
echo

echo "10. Consultando saldo da segunda conta após transferência..."
SALDO2_FINAL=$(curl -s -X GET "$CONTA_API/api/ContaCorrente/saldo" \
  -H "Authorization: Bearer $TOKEN2")

echo "Resposta: $SALDO2_FINAL"
echo

echo "=== Teste concluído! ==="
echo "A API de Tarifas processará automaticamente a transferência via Kafka."
echo "Verifique os logs da API de Tarifas para ver a tarifa sendo aplicada."
