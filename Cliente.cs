using System;

namespace SistemaQuadras
{
    // Classe de modelo: representa o cliente que aluga a quadra
    class Cliente
    {
        public string Nome;
        public string Telefone;

        public Cliente(string nome, string telefone)
        {
            Nome = nome;
            Telefone = telefone;
        }
    }
}
