using AluguelQuadras.Models;
using Microsoft.EntityFrameworkCore;

namespace AluguelQuadras.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Quadra> Quadras { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Aluguel> Alugueis { get; set; }
}
