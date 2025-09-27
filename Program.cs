// Program.cs

// 1. Los 'using' van siempre al principio del archivo.
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EC2_PROGRA1.Data;

// 2. Creamos el constructor de la aplicación (SOLO UNA VEZ).
var builder = WebApplication.CreateBuilder(args);

// 3. Obtenemos la cadena de conexión del archivo appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 4. Registramos el DbContext con Entity Framework Core, especificando que usaremos SQLite.
//    Esto permite que la aplicación sepa cómo conectarse a la base de datos.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)); 

// Esto es útil para ver errores detallados de la base de datos durante el desarrollo.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 5. Configuramos Identity (manejo de usuarios, roles, login, etc.)
//    Le decimos que use nuestro ApplicationDbContext para guardar la información de los usuarios.
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 6. Añadimos el servicio para que el proyecto reconozca el patrón MVC (Controladores y Vistas).
builder.Services.AddControllersWithViews();

// 7. Construimos la aplicación.
var app = builder.Build();

// 8. Configuramos el "pipeline" de peticiones HTTP (cómo se procesa una solicitud del navegador).
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Parte de AddDatabaseDeveloperPageExceptionFilter
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Permite servir archivos estáticos como CSS, JS e imágenes desde wwwroot.

app.UseRouting();

app.UseAuthentication(); // Importante: Autenticación va antes de Autorización.
app.UseAuthorization();

// 9. Definimos la ruta por defecto.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Necesario para las páginas de Identity (Login, Register, etc.)

// 10. Ejecutamos la aplicación.

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        // Solo sembramos si no hay ningún inmueble en la base de datos.
        if (!context.Inmuebles.Any())
        {
            context.Inmuebles.AddRange(
                new EC2_PROGRA1.Models.Inmueble
                {
                    Codigo = "DEP001",
                    Titulo = "Moderno Departamento en el Centro",
                    Tipo = EC2_PROGRA1.Models.TipoInmueble.Departamento,
                    Ciudad = "Bogotá",
                    Direccion = "Calle 100 #10-20",
                    Dormitorios = 2,
                    Banos = 2,
                    MetrosCuadrados = 85,
                    Precio = 350000000,
                    Imagen = "/images/depa1.jpg", // Ruta relativa a wwwroot
                    Activo = true
                },
                new EC2_PROGRA1.Models.Inmueble
                {
                    Codigo = "CAS001",
                    Titulo = "Amplia Casa con Jardín",
                    Tipo = EC2_PROGRA1.Models.TipoInmueble.Casa,
                    Ciudad = "Medellín",
                    Direccion = "Carrera 43 #25-15",
                    Dormitorios = 4,
                    Banos = 3,
                    MetrosCuadrados = 220,
                    Precio = 800000000,
                    Imagen = "/images/casa1.jpg",
                    Activo = true
                },
                new EC2_PROGRA1.Models.Inmueble
                {
                    Codigo = "OFI001",
                    Titulo = "Oficina con Vista Panorámica",
                    Tipo = EC2_PROGRA1.Models.TipoInmueble.Oficina,
                    Ciudad = "Bogotá",
                    Direccion = "Avenida El Dorado #50-50",
                    Dormitorios = 0,
                    Banos = 1,
                    MetrosCuadrados = 50,
                    Precio = 250000000,
                    Imagen = "/images/oficina1.jpg",
                    Activo = true
                },
                new EC2_PROGRA1.Models.Inmueble
                {
                    Codigo = "LOC001",
                    Titulo = "Local Comercial en Zona concurrida",
                    Tipo = EC2_PROGRA1.Models.TipoInmueble.Local,
                    Ciudad = "Cali",
                    Direccion = "Calle 5 #66-80",
                    Dormitorios = 0,
                    Banos = 1,
                    MetrosCuadrados = 100,
                    Precio = 450000000,
                    Imagen = "/images/local1.jpg",
                    Activo = false // Un ejemplo inactivo para pruebas futuras
                }
            );
            context.SaveChanges(); // Guardamos los cambios en la BD
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Un error ocurrió al sembrar la base de datos.");
    }
}
// --- FIN: Bloque para sembrar datos ---

// Esta línea debe estar después del bloque anterior

app.Run();