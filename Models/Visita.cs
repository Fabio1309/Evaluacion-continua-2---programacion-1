// Models/Visita.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EC2_PROGRA1.Models;

public enum EstadoVisita
{
    Solicitada, Confirmada, Cancelada
}

public class Visita
{
    public int Id { get; set; }

    [Required]
    public int InmuebleId { get; set; }
    public Inmueble Inmueble { get; set; } = null!; // Corregido

    [Required]
    public string UsuarioId { get; set; } = null!; // Corregido
    public IdentityUser Usuario { get; set; } = null!; // Corregido

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    public EstadoVisita Estado { get; set; } = EstadoVisita.Solicitada;

    public string? Notas { get; set; }
}