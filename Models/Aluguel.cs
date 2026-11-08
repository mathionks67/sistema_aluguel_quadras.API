namespace AluguelQuadras.Models;

public class Aluguel
{
    public int Id { get; set; }

    // Relacionamento: cada aluguel pertence a UMA quadra e a UM cliente.
    public int QuadraId { get; set; }
    public Quadra Quadra { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }

    public int Horas { get; set; }

    // Guardado no momento do aluguel: se o preço da quadra mudar depois,
    // este aluguel continua com o valor que foi cobrado.
    public double ValorTotal { get; set; }

    public DateTime DataInicio { get; set; }

    // null = aluguel ativo. Preenchido = aluguel encerrado.
    public DateTime? DataFim { get; set; }
}
