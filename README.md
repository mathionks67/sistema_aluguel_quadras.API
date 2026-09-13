# Sistema de Aluguel de Quadras Esportivas

## Descrição
Sistema em (C#) que simula o aluguel de quadras esportivas (futsal, vôlei,
basquete, etc.) de um centro esportivo. Resolve um problema comum do dia a dia: controlar
quais quadras estão disponíveis, registrar quem alugou cada uma, calcular o valor a pagar
e liberar a quadra quando o aluguel termina.

## Integrantes
- Pedro Henrique Souza Santos
- Igor Vinicius Dias Ramos
- Matheus de Faria Giraldelo

## Funcionalidades
- Cadastrar quadras (nome, esporte, valor por hora)
- Listar todas as quadras e sua situação (disponível/alugada)
- Buscar quadra pelo nome
- Alugar uma quadra para um cliente, informando a quantidade de horas
- Listar os aluguéis ativos
- Encerrar um aluguel, liberando a quadra novamente

### Regras de negócio implementadas
- Não é possível alugar uma quadra que já está ocupada.
- A quantidade de horas do aluguel é limitada (mínimo 1, máximo 4 horas).
- O valor total do aluguel é calculado automaticamente (valor da hora x horas).
- Não é permitido cadastrar quadra com nome vazio ou valor por hora negativo.

## Tecnologias
- C#
- .NET 8
- Git/GitHub

## Como executar
1. Clonar o repositório.
2. Abrir a pasta do projeto no VS Code.
3. Abrir um terminal na pasta do projeto e executar:
   ```
   dotnet run
   ```
4. Interagir com o sistema pelo menu exibido no console.
