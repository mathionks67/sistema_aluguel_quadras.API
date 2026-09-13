using System;

namespace SistemaQuadras
{
    // Classe que representa o relacionamento entre Cliente e Quadra
    class Aluguel
    {
        public Quadra Quadra;
        public Cliente Cliente;
        public int Horas;
        public double ValorTotal;

        public Aluguel(Quadra quadra, Cliente cliente, int horas)
        {
            Quadra = quadra;
            Cliente = cliente;
            Horas = horas;
            ValorTotal = quadra.ValorHora * horas; // regra de negócio: cálculo do valor
        }
    }
}
