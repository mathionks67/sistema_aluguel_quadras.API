namespace AluguelQuadras.Models;

public class Quadra
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Esporte { get; set; }
    public double ValorHora { get; set; }
    public bool Disponivel { get; set; } = true;
}
