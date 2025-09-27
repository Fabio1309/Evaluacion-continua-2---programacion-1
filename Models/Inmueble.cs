// Models/Inmueble.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EC2_PROGRA1.Models;

public enum TipoInmueble
{
    Departamento,
    Casa,
    Oficina,
    Local
}

[Index(nameof(Codigo), IsUnique = true)]
public class Inmueble
{
    public int Id { get; set; }

    [Required]
    public string Codigo { get; set; }

    [Required]
    public string Titulo { get; set; }

    public string? Imagen { get; set; }

    [Required]
    public TipoInmueble Tipo { get; set; }

    [Required]
    public string Ciudad { get; set; }

    [Required]
    public string Direccion { get; set; }

    public int Dormitorios { get; set; }

    public int Banos { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Los metros cuadrados deben ser mayores a 0.")]
    public int MetrosCuadrados { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;

    // Propiedades de navegación
    public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}