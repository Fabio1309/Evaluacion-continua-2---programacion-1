// Data/ApplicationDbContext.cs
using EC2_PROGRA1.Models; // Añade esta línea
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EC2_PROGRA1.Data;

public class ApplicationDbContext : IdentityDbContext
{
    // Añade estas tres líneas para tus modelos
    public DbSet<Inmueble> Inmuebles { get; set; }
    public DbSet<Visita> Visitas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}