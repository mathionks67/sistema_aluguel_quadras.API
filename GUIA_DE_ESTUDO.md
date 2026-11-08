# Guia de estudo - Sistema de Aluguel de Quadras (API)

Este guia explica o código em linguagem simples. Leia, depois feche o guia e tente explicar
cada parte **com as suas palavras** olhando só para o código. Não decore: entenda o caminho.

---

## 1. O caminho dos dados (a coisa mais importante)

Exemplo: criar um aluguel.

```
Postman envia: POST http://localhost:5000/alugueis
               corpo JSON: { "quadraId": 1, "clienteId": 1, "horas": 2 }
        |
        v
O ASP.NET encontra a rota MapPost("/alugueis") no Program.cs
        |
        v
O JSON é convertido (desserializado) em um objeto NovoAluguel
        |
        v
O código valida (horas), busca a quadra e o cliente no banco (context.Quadras.Find...)
        |
        v
Cria o objeto Aluguel, calcula o ValorTotal, marca a quadra como indisponível
        |
        v
context.SaveChanges()  ->  o Entity Framework gera o INSERT / UPDATE e grava no SQLite
        |
        v
A API devolve Results.Created(...)  ->  status 201 + o aluguel convertido em JSON
```

**Como explicar ao professor:** "O cliente manda um JSON por HTTP, o endpoint da Minimal API
valida as regras, usa o DbContext do Entity Framework para ler e gravar no SQLite, e devolve
um JSON com um status HTTP."

---

## 2. Arquivo por arquivo

### AluguelQuadras.csproj
Configuração do projeto. Diz que é .NET 8, que é um projeto Web (necessário para Minimal API)
e quais pacotes usa: `EntityFrameworkCore.Sqlite` (conversa com o SQLite) e
`EntityFrameworkCore.Design` (permite criar migrations).
*Detalhe de implementação.* Só saiba dizer para que servem os dois pacotes.

### Models/Quadra.cs, Cliente.cs, Aluguel.cs (as entidades)
Cada classe vira uma tabela. Cada propriedade vira uma coluna.
- `Id` é a chave primária: o banco gera o número sozinho.
- `Aluguel` tem `QuadraId` + `Quadra` e `ClienteId` + `Cliente`. Isso é o **relacionamento**:
  um aluguel pertence a uma quadra e a um cliente; uma quadra (ou cliente) pode ter vários aluguéis.
  `QuadraId` é a **chave estrangeira** (o número que aponta para a quadra); `Quadra` é o
  objeto ligado, usado para acessar os dados dela no C#.
- `DataFim` é `DateTime?` (aceita nulo). Nulo = aluguel ativo.
- `ValorTotal` fica gravado no aluguel para manter o valor cobrado, mesmo que o preço da quadra mude depois.
- `Disponivel` em `Quadra` vem do projeto original. É atualizado junto com o aluguel, no mesmo `SaveChanges`.

**Saiba explicar:** o que é entidade, chave primária, chave estrangeira, relacionamento 1 para muitos.

### Data/AppDbContext.cs
É a "ponte" entre o C# e o banco. Cada `DbSet<>` representa uma tabela
(`context.Quadras` = tabela Quadras). Sempre que o código precisa do banco, usa o `AppDbContext`.

**Saiba explicar:** o que é DbContext, o que é DbSet.

### Dtos/Pedidos.cs
Três `record`s que definem o formato do JSON de **entrada** (`NovaQuadra`, `NovoCliente`, `NovoAluguel`).
Por que existem? Para o usuário informar só o que pode. Se ele mandasse a `Quadra` inteira,
poderia enviar `Disponivel` ou `ValorTotal` por conta própria. Assim o `ValorTotal` é sempre
calculado pela API.

**Saiba explicar:** por que não recebemos a entidade diretamente (segurança e regra de negócio).

### Program.cs
Tem três partes:
1. **Configuração:** `AddDbContext` registra o banco (SQLite, arquivo `aluguel.db`). Isso é a
   *injeção de dependência*: nos endpoints basta pedir `AppDbContext context` e o ASP.NET entrega pronto.
2. **Endpoints** (`MapGet`, `MapPost`, `MapPut`): cada um liga uma rota HTTP a um trecho de código.
3. `app.Run()`: liga o servidor.

Padrão dos endpoints (igual aos slides): validar -> buscar no banco -> alterar -> `SaveChanges()` -> devolver `Results.X`.

**Saiba explicar:** o que é Minimal API, o que é endpoint, o que cada verbo faz, por que cada status foi usado.

### Migrations/ (gerada pelo comando, não escrita à mão)
O histórico das mudanças do banco. `migrations add` escreve o "roteiro" (arquivos C#);
`database update` executa o roteiro no banco e cria as tabelas.
*Não precisa decorar o conteúdo.* Precisa saber para que serve.

### .gitignore
Impede de enviar `bin/`, `obj/` e `*.db` ao GitHub. O banco não vai junto: cada pessoa recria com `database update`.
*Detalhe.*

---

## 3. Os endpoints (o que cada um faz e por quê)

- **POST /quadras:** cadastra. Valida nome vazio e valor negativo (400). Devolve 201 (criado) com a quadra.
- **GET /quadras:** lista. `?nome=` busca por parte do nome. `?disponivel=true` filtra as livres.
  Esse mesmo endpoint cobre "listar", "buscar" e "ver disponibilidade".
- **GET /quadras/{id}:** uma quadra. Se não existe, 404.
- **POST /clientes** e **GET /clientes:** cadastra e lista clientes.
- **POST /alugueis:** o mais importante. Verifica horas (400), se a quadra e o cliente existem (404),
  se a quadra está livre (409). Calcula o total, cria o aluguel e deixa a quadra indisponível.
- **GET /alugueis/ativos:** aluguéis com `DataFim` vazia.
- **GET /alugueis/{id}:** um aluguel (404 se não existe). Existe também porque o 201 do POST aponta para essa rota.
- **PUT /alugueis/{id}/encerrar:** preenche `DataFim` e libera a quadra. PUT porque altera algo que já existe.
  Não usamos DELETE porque o aluguel continua no histórico.

---

## 4. Conceitos em poucas frases

- **API:** programa que outros programas acessam por HTTP para pedir ou enviar dados.
- **REST:** jeito de organizar a API: cada coisa tem um endereço (`/quadras`) e o verbo HTTP diz a ação.
- **Endpoint:** um endereço + um verbo. Ex.: `POST /alugueis`.
- **Verbos:** GET consulta; POST cria; PUT altera; DELETE apaga.
- **JSON:** texto que representa objetos, ex.: `{ "nome": "Quadra 1", "valorHora": 80 }`.
  O .NET converte objeto <-> JSON sozinho.
- **Minimal API:** forma enxuta de criar endpoints, sem controllers: `app.MapGet("/rota", () => ...)`.
- **ORM:** transforma classes em tabelas e objetos em linhas, para não escrever SQL à mão.
- **Entity Framework Core:** o ORM da Microsoft.
- **SQLite:** banco de dados em um arquivo, sem instalar servidor.
- **SaveChanges:** é o que realmente grava no banco. `Add` e alterações só preparam.
- **Migration:** histórico das mudanças do banco, gerado a partir das classes.
- **Status HTTP:** 200 OK; 201 criado; 400 pedido inválido; 404 não encontrado; 409 conflito.

---

## 5. Perguntas prováveis da defesa (respostas curtas de apoio)

Tente responder **antes** de ler a resposta. Depois reescreva com suas palavras.

1. **O que é REST?** Estilo de API em que cada recurso tem uma URL e os verbos HTTP indicam a ação.
2. **O que é JSON e como a API o usa?** Formato de texto para dados. A API recebe JSON, converte em objeto C#, e devolve objetos convertidos em JSON.
3. **O que é Minimal API?** Jeito simples do .NET de criar endpoints direto no `Program.cs`, sem controllers.
4. **Diferença entre GET, POST e PUT?** GET consulta, POST cria, PUT altera um recurso existente.
5. **Por que usou PUT para encerrar e não DELETE?** Porque o aluguel não é apagado: só ganha uma data de fim e libera a quadra.
6. **O que significa 201? E 400, 404, 409?** 201 criado. 400 dados inválidos. 404 não encontrado. 409 conflito com o estado atual (quadra já alugada).
7. **Por que 409 para quadra ocupada e não 400?** O pedido está bem formado; o conflito é com o estado do sistema.
8. **O que é Entity Framework Core?** ORM: deixa trabalhar com objetos C# em vez de SQL.
9. **O que é DbContext?** Classe que faz a ponte entre o código e o banco.
10. **O que é DbSet?** Representa uma tabela dentro do DbContext.
11. **O que é migration?** Histórico das alterações do banco. `migrations add` gera o roteiro; `database update` aplica no banco.
12. **Por que rodar `database update`?** Sem ele, o arquivo `aluguel.db` e as tabelas não existem.
13. **Por que SQLite?** Foi o usado na aula, é um arquivo local e não exige instalar nada.
14. **O que é SaveChanges?** Executa no banco o que foi preparado (INSERT, UPDATE).
15. **Como os dados continuam após reiniciar?** Estão gravados no arquivo `aluguel.db`, não em listas na memória.
16. **Como funciona o relacionamento entre Aluguel, Quadra e Cliente?** `Aluguel` guarda `QuadraId` e `ClienteId` (chaves estrangeiras). Uma quadra ou cliente pode ter vários aluguéis; cada aluguel tem uma quadra e um cliente.
17. **O que é injeção de dependência?** O ASP.NET cria o `AppDbContext` e entrega nos endpoints que pedem esse parâmetro.
18. **Quais regras de negócio existem e onde estão?** Nome vazio, valor negativo, horas de 1 a 4, quadra ocupada, aluguel já encerrado. Estão nos endpoints do `Program.cs`.
19. **Como o valor total é calculado?** `ValorHora x Horas`, na API, no momento do aluguel, e fica gravado.
20. **Por que `Disponivel` fica na quadra se dá para saber pelos aluguéis?** Veio do projeto original e simplifica a consulta. O risco é ficar inconsistente, por isso é atualizado junto com o aluguel no mesmo `SaveChanges`.
21. **Por que existem os DTOs (`NovoAluguel`)?** Para o usuário enviar só os campos permitidos; o resto é decidido pela API.
22. **O que mudou do projeto antigo (console) para este?** Saiu o menu e as listas em memória. Entraram endpoints HTTP e o banco com EF. As regras foram mantidas.
23. **Como você testou?** Pelo Postman, seguindo os 4 casos de uso do README.

---

## 6. Decisões para confirmar com a equipe
- Manter `Cliente` como entidade com `Id` (cadastro separado) ou pedir nome/telefone dentro do aluguel.
- Manter `Disponivel` na quadra (como está) ou calcular só pelos aluguéis ativos.
- Quem apresenta cada parte na defesa. **Todos precisam saber explicar o fluxo completo.**
- Se o professor quer o arquivo `aluguel.db` na entrega ou só as instruções (`database update`).

## 7. Subir no Git (passo a passo)
Opção A: usar o repositório antigo.
1. Copie a pasta oculta `.git` do projeto antigo (`TRABALHO_TERÇA`) para dentro da pasta `AluguelQuadras`.
2. No terminal do VS Code: `git status`. Se aparecer merge em andamento, resolva antes (`git merge --abort` ou conclua com `git commit`).
3. `git add .`
4. `git commit -m "Transforma o sistema em API com Minimal API e Entity Framework"`
5. `git push`

A pasta `Migrations/` **deve** ir para o Git. O arquivo `aluguel.db` não (já está no `.gitignore`).
