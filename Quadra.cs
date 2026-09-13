using System;

namespace SistemaQuadras
{
    // Classe principal de modelo: representa uma quadra esportiva
    class Quadra
    {
        public int Id;
        public string Nome;
        public string Esporte;
        public double ValorHora;
        public bool Disponivel;

        public Quadra(int id, string nome, string esporte, double valorHora)
        {
            Id = id;
            Nome = nome;
            Esporte = esporte;
            ValorHora = valorHora;
            Disponivel = true; // toda quadra nasce disponível
        }
    }
}
