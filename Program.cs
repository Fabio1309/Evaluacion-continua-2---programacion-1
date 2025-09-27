// Program.cs - VERSIÓN COMPLETA Y CORREGIDA

// --- 1. Usings necesarios ---
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EC2_PROGRA1.Data;
using EC2_PROGRA1.Models; // Asegúrate de tener este using

// --- 2. Creación del WebApplication Builder ---
var builder = WebApplication.CreateBuilder(args);

// --- 3. Configuración de Servicios en el Contenedor de Inyección de Dependencias ---

// 3.1 - Conexión a la Base de Datos (Entity Framework Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 3.2 - Herramientas de Desarrollo para la Base de Datos
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 3.3 - Configuración de ASP.NET Core Identity (Usuarios y Roles)
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Habilita el manejo de Roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3.4 - Configuración de MVC (Controladores y Vistas)
builder.Services.AddControllersWithViews();

// --- 4. Construcción de la Aplicación ---
var app = builder.Build();

// --- 5. Lógica de Inicialización (Siembra de Datos y Creación de Roles) ---
// Se ejecuta una sola vez al iniciar la aplicación.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // --- LÓGICA PARA ROLES ---
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Crear el rol "Broker" si no existe
        if (!await roleManager.RoleExistsAsync("Broker"))
        {
            await roleManager.CreateAsync(new IdentityRole("Broker"));
            logger.LogInformation("Rol 'Broker' creado.");
        }

        // Asignar el usuario principal al rol de Broker
        var brokerEmail = "skydragons001@gmail.com"; 
        var brokerUser = await userManager.FindByEmailAsync(brokerEmail);

        if (brokerUser != null && !await userManager.IsInRoleAsync(brokerUser, "Broker"))
        {
            await userManager.AddToRoleAsync(brokerUser, "Broker");
            logger.LogInformation($"Usuario {brokerEmail} asignado al rol 'Broker'.");
        }
        else if (brokerUser == null)
        {
            logger.LogWarning($"Usuario {brokerEmail} no fue encontrado para asignarle el rol de Broker. Por favor, regístralo primero.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Un error ocurrió al crear roles y asignar el usuario Broker.");
    }

    // --- LÓGICA PARA SEMBRAR INMUEBLES ---
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        if (!context.Inmuebles.Any())
        {
            context.Inmuebles.AddRange(
                new EC2_PROGRA1.Models.Inmueble {
                    Codigo = "DEP001", Titulo = "Moderno Departamento en el Centro", Tipo = EC2_PROGRA1.Models.TipoInmueble.Departamento,
                    Ciudad = "Bogotá", Direccion = "Calle 100 #10-20", Dormitorios = 2, Banos = 2, MetrosCuadrados = 85,
                    Precio = 350000000, Imagen = "/images/depa1.jpg", Activo = true
                },
                new EC2_PROGRA1.Models.Inmueble {
                    Codigo = "CAS001", Titulo = "Amplia Casa con Jardín", Tipo = EC2_PROGRA1.Models.TipoInmueble.Casa,
                    Ciudad = "Medellín", Direccion = "Carrera 43 #25-15", Dormitorios = 4, Banos = 3, MetrosCuadrados = 220,
                    Precio = 800000000, Imagen = "/images/casa1.jpg", Activo = true
                },
                new EC2_PROGRA1.Models.Inmueble {
                    Codigo = "OFI001", Titulo = "Oficina con Vista Panorámica", Tipo = EC2_PROGRA1.Models.TipoInmueble.Oficina,
                    Ciudad = "Bogotá", Direccion = "Avenida El Dorado #50-50", Dormitorios = 0, Banos = 1, MetrosCuadrados = 50,
                    Precio = 250000000, Imagen = "/images/oficina1.jpg", Activo = true
                },
                new EC2_PROGRA1.Models.Inmueble {
                    Codigo = "LOC001", Titulo = "Local Comercial en Zona concurrida", Tipo = EC2_PROGRA1.Models.TipoInmueble.Local,
                    Ciudad = "Cali", Direccion = "Calle 5 #66-80", Dormitorios = 0, Banos = 1, MetrosCuadrados = 100,
                    Precio = 450000000, Imagen = "/images/local1.jpg", Activo = false
                }
            );
            context.SaveChanges();
            logger.LogInformation("Base de datos sembrada con inmuebles iniciales.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Un error ocurrió al sembrar la base de datos de inmuebles.");
    }
}


// --- 6. Configuración del Pipeline de Peticiones HTTP ---

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// --- 7. Ejecución de la Aplicación ---
app.Run();