using AluguelQuadras.Data;
using AluguelQuadras.Dtos;
using AluguelQuadras.Models;
using Microsoft.EntityFrameworkCore;

const int HORAS_MAXIMAS = 4;

var builder = WebApplication.CreateBuilder(args);

// Registra o DbContext e diz que o banco é um arquivo SQLite local.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=aluguel.db");
});

var app = builder.Build();

// Rota de teste, só para confirmar que a API subiu.
app.MapGet("/", () => "API de Aluguel de Quadras funcionando!");

// ======================= QUADRAS =======================

// Cadastrar quadra
app.MapPost("/quadras", (AppDbContext context, NovaQuadra dados) =>
{
    if (string.IsNullOrWhiteSpace(dados.Nome))
        return Results.BadRequest("O nome da quadra não pode ser vazio.");

    if (dados.ValorHora < 0)
        return Results.BadRequest("O valor por hora não pode ser negativo.");

    var quadra = new Quadra
    {
        Nome = dados.Nome,
        Esporte = dados.Esporte,
        ValorHora = dados.ValorHora,
        Disponivel = true
    };

    context.Quadras.Add(quadra);
    context.SaveChanges();

    return Results.Created($"/quadras/{quadra.Id}", quadra);
});

// Listar quadras. Filtros opcionais:
//   /quadras?nome=society      -> busca pelo nome (ou parte dele)
//   /quadras?disponivel=true   -> só as disponíveis
app.MapGet("/quadras", (AppDbContext context, string nome = null, bool? disponivel = null) =>
{
    var consulta = context.Quadras.AsQueryable();

    if (!string.IsNullOrWhiteSpace(nome))
    {
        var busca = nome.ToLower();
        consulta = consulta.Where(q => q.Nome.ToLower().Contains(busca));
    }

    if (disponivel.HasValue)
    {
        var valor = disponivel.Value;
        consulta = consulta.Where(q => q.Disponivel == valor);
    }

    return Results.Ok(consulta.ToList());
});

// Consultar uma quadra pelo Id
app.MapGet("/quadras/{id:int}", (AppDbContext context, int id) =>
{
    var quadra = context.Quadras.Find(id);

    if (quadra == null)
        return Results.NotFound("Quadra não encontrada.");

    return Results.Ok(quadra);
});

// ======================= CLIENTES =======================

// Cadastrar cliente
app.MapPost("/clientes", (AppDbContext context, NovoCliente dados) =>
{
    if (string.IsNullOrWhiteSpace(dados.Nome))
        return Results.BadRequest("O nome do cliente não pode ser vazio.");

    var cliente = new Cliente
    {
        Nome = dados.Nome,
        Telefone = dados.Telefone
    };

    context.Clientes.Add(cliente);
    context.SaveChanges();

    return Results.Created($"/clientes/{cliente.Id}", cliente);
});

// Listar clientes (serve para descobrir o Id ao criar um aluguel)
app.MapGet("/clientes", (AppDbContext context) =>
{
    return Results.Ok(context.Clientes.ToList());
});

// ======================= ALUGUÉIS =======================

// Criar aluguel. Ordem das verificações: 400 (horas) -> 404 (não existe) -> 409 (ocupada)
app.MapPost("/alugueis", (AppDbContext context, NovoAluguel dados) =>
{
    if (dados.Horas < 1 || dados.Horas > HORAS_MAXIMAS)
        return Results.BadRequest($"Quantidade de horas inválida. Informe de 1 a {HORAS_MAXIMAS}.");

    var quadra = context.Quadras.Find(dados.QuadraId);
    if (quadra == null)
        return Results.NotFound("Quadra não encontrada.");

    var cliente = context.Clientes.Find(dados.ClienteId);
    if (cliente == null)
        return Results.NotFound("Cliente não encontrado.");

    if (!quadra.Disponivel)
        return Results.Conflict("Essa quadra já está alugada no momento.");

    var aluguel = new Aluguel
    {
        Quadra = quadra,
        Cliente = cliente,
        Horas = dados.Horas,
        ValorTotal = quadra.ValorHora * dados.Horas,
        DataInicio = DateTime.Now
    };

    // Aluguel novo e quadra ocupada são salvos juntos, no mesmo SaveChanges.
    quadra.Disponivel = false;
    context.Alugueis.Add(aluguel);
    context.SaveChanges();

    return Results.Created($"/alugueis/{aluguel.Id}", aluguel);
});

// Listar aluguéis ativos (DataFim vazia = ainda não encerrado)
app.MapGet("/alugueis/ativos", (AppDbContext context) =>
{
    var ativos = context.Alugueis
        .Include(a => a.Quadra)
        .Include(a => a.Cliente)
        .Where(a => a.DataFim == null)
        .ToList();

    return Results.Ok(ativos);
});

// Consultar um aluguel pelo Id
app.MapGet("/alugueis/{id:int}", (AppDbContext context, int id) =>
{
    var aluguel = context.Alugueis
        .Include(a => a.Quadra)
        .Include(a => a.Cliente)
        .FirstOrDefault(a => a.Id == id);

    if (aluguel == null)
        return Results.NotFound("Aluguel não encontrado.");

    return Results.Ok(aluguel);
});

// Encerrar aluguel e liberar a quadra.
// Usa PUT porque altera um aluguel existente; o aluguel não é apagado (fica no histórico).
app.MapPut("/alugueis/{id:int}/encerrar", (AppDbContext context, int id) =>
{
    var aluguel = context.Alugueis
        .Include(a => a.Quadra)
        .Include(a => a.Cliente)
        .FirstOrDefault(a => a.Id == id);

    if (aluguel == null)
        return Results.NotFound("Aluguel não encontrado.");

    if (aluguel.DataFim != null)
        return Results.Conflict("Esse aluguel já foi encerrado.");

    aluguel.DataFim = DateTime.Now;
    aluguel.Quadra.Disponivel = true;
    context.SaveChanges();

    return Results.Ok(aluguel);
});

app.Run();
