// Models/Reserva.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EC2_PROGRA1.Models;

public class Reserva
{
    public int Id { get; set; }

    [Required]
    public int InmuebleId { get; set; }
    public Inmueble Inmueble { get; set; } = null!; // Corregido

    [Required]
    public string UsuarioId { get; set; } = null!; // Corregido
    public IdentityUser Usuario { get; set; } = null!; // Corregido

    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}