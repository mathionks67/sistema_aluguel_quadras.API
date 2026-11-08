# Sistema de Aluguel de Quadras Esportivas (API)

## Descrição
API HTTP em C# (.NET 8) para controlar o aluguel de quadras esportivas (futsal, vôlei,
basquete etc.): cadastrar quadras e clientes, ver quais quadras estão disponíveis, alugar uma
quadra, calcular o valor a pagar e liberar a quadra quando o aluguel termina.
Os dados ficam salvos em um banco SQLite, usando Entity Framework Core.

## Integrantes
- Pedro Henrique Souza Santos
- Igor Vinicius Dias Ramos
- Matheus de Faria Giraldelo

## Tecnologias
- C# / .NET 8
- Minimal API
- Entity Framework Core 8 + SQLite
- Git/GitHub

## Como executar
Pré-requisito: .NET 8 SDK instalado (`dotnet --version` deve começar com `8.`).

```
dotnet tool install --global dotnet-ef --version 8.*
dotnet restore
dotnet ef database update
dotnet run
```

- `dotnet ef database update` cria o arquivo `aluguel.db` aplicando as migrations da pasta `Migrations/`.
- A API sobe em `http://localhost:5000` (confira a linha "Now listening on" no terminal).
- Execute os comandos dentro da pasta do projeto (onde está o `.csproj`).
- Se o `dotnet-ef` já estiver instalado, use `dotnet tool update --global dotnet-ef --version 8.*`.

## Regras de negócio
- Não é possível alugar uma quadra que já está ocupada (409).
- A quantidade de horas é de 1 a 4 (400).
- O valor total é calculado pela API: valor da hora x horas.
- Não é permitido cadastrar quadra com nome vazio ou valor por hora negativo (400).
- Encerrar um aluguel libera a quadra; um aluguel já encerrado não pode ser encerrado de novo (409).

## Endpoints

| Método | Rota | Finalidade | Status |
|---|---|---|---|
| POST | `/quadras` | Cadastrar quadra | 201, 400 |
| GET | `/quadras` | Listar quadras. Filtros: `?nome=` e `?disponivel=true` | 200 |
| GET | `/quadras/{id}` | Consultar uma quadra | 200, 404 |
| POST | `/clientes` | Cadastrar cliente | 201, 400 |
| GET | `/clientes` | Listar clientes | 200 |
| POST | `/alugueis` | Criar aluguel | 201, 400, 404, 409 |
| GET | `/alugueis/ativos` | Listar aluguéis ativos | 200 |
| GET | `/alugueis/{id}` | Consultar um aluguel | 200, 404 |
| PUT | `/alugueis/{id}/encerrar` | Encerrar aluguel e liberar a quadra | 200, 404, 409 |

### Exemplos de JSON

**POST /quadras**
```json
{ "nome": "Quadra Society 1", "esporte": "Futebol", "valorHora": 80 }
```
Resposta (201):
```json
{ "id": 1, "nome": "Quadra Society 1", "esporte": "Futebol", "valorHora": 80, "disponivel": true }
```

**POST /clientes**
```json
{ "nome": "Maria Silva", "telefone": "41 99999-0000" }
```
Resposta (201):
```json
{ "id": 1, "nome": "Maria Silva", "telefone": "41 99999-0000" }
```

**POST /alugueis**
```json
{ "quadraId": 1, "clienteId": 1, "horas": 2 }
```
Resposta (201):
```json
{
  "id": 1,
  "quadraId": 1,
  "quadra": { "id": 1, "nome": "Quadra Society 1", "esporte": "Futebol", "valorHora": 80, "disponivel": false },
  "clienteId": 1,
  "cliente": { "id": 1, "nome": "Maria Silva", "telefone": "41 99999-0000" },
  "horas": 2,
  "valorTotal": 160,
  "dataInicio": "2026-11-09T14:30:00",
  "dataFim": null
}
```

**Erros** (corpo é uma mensagem de texto):
- 400: `Quantidade de horas inválida. Informe de 1 a 4.`
- 404: `Quadra não encontrada.`
- 409: `Essa quadra já está alugada no momento.`

## Casos de uso para demonstração
Use o Postman (ou outro cliente HTTP). Banco vazio no início.

**Caso 1 - Cadastrar e consultar quadras**
1. `POST /quadras` com "Quadra Society 1" / Futebol / 80 -> 201, id 1.
2. `POST /quadras` com "Quadra de Vôlei" / Vôlei / 50 -> 201, id 2.
3. `GET /quadras` -> lista com as 2 quadras.
4. `GET /quadras?nome=society` -> só a Society.
5. `GET /quadras?disponivel=true` -> as 2 (nenhuma alugada ainda).

**Caso 2 - Realizar um aluguel**
1. `POST /clientes` com Maria Silva -> 201, id 1.
2. `POST /alugueis` com quadraId 1, clienteId 1, horas 2 -> 201, valorTotal 160.
3. `GET /quadras?disponivel=true` -> só a Quadra de Vôlei (a Society está ocupada).
4. `GET /alugueis/ativos` -> o aluguel da Maria.

**Caso 3 - Regras de negócio**
1. `POST /alugueis` com quadraId 1 (ocupada) -> 409 "Essa quadra já está alugada no momento."
2. `POST /alugueis` com quadraId 2, clienteId 1, horas 5 -> 400 (horas inválidas).
3. `POST /alugueis` com quadraId 99 -> 404 "Quadra não encontrada."

**Caso 4 - Encerrar aluguel e provar a persistência**
1. `PUT /alugueis/1/encerrar` -> 200, `dataFim` preenchida.
2. `GET /alugueis/ativos` -> lista vazia `[]`.
3. `GET /quadras?disponivel=true` -> as 2 quadras de novo.
4. Pare a API (Ctrl+C) e rode `dotnet run` de novo.
5. `GET /quadras` e `GET /clientes` -> os dados continuam lá (persistência no SQLite).

## Limitações conhecidas
- Não há agenda por data/hora: "disponível" significa "sem aluguel ativo agora".
- Não há autenticação nem login.
- Não há endpoints para editar ou excluir quadras e clientes.
- A busca por nome ignora maiúsculas/minúsculas, mas o SQLite só faz isso bem para letras sem acento.
- Valores em dinheiro usam `double`, como no projeto original (em um sistema real seria `decimal`).
