using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using QuestPDF.Infrastructure;
using GestorCuentasCorrientes.web.Services;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar Identity con Usuario personalizado
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    // Opciones de contraseña razonables para sistemas administrativos internos
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Opciones de cuenta
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurar rutas de autenticación para Razor Pages
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<ReciboPdfService>();
//Definir la cultura predeterminada para toda la aplicación (es-AR) para moneda, fechas y otros formatos regionales
var culturaArgentina = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = culturaArgentina;
CultureInfo.DefaultThreadCurrentUICulture = culturaArgentina;

var app = builder.Build();

// Ejecutar seeder de roles, usuarios y localidades
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await SeedRolesAsync(roleManager);
    await SeedUsersAsync(userManager);
    await SeedLocalidadesAsync(context);
    await SeedProductosAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Mapear Razor Pages PRIMERO (para Identity Pages)
app.MapRazorPages();

// Mapear Controllers MVC después
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Método seeder para roles
async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    var roles = new[] { "Admin", "Operador" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Método seeder para usuarios
async Task SeedUsersAsync(UserManager<Usuario> userManager)
{
    const string adminEmail = "admin@gmail.com";
    const string adminPassword = "Admin123";

    // Verificar si ya existe un usuario con rol Admin
    var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
    if (adminUsers.Count > 0)
    {
        return; // Ya existe un usuario Admin, no crear otro
    }

    // Crear usuario Admin de prueba
    var adminUser = new Usuario
    {
        UserName = adminEmail,
        Email = adminEmail,
        Nombre = "Administrador",
        Apellido = "Sistema"
    };

    var result = await userManager.CreateAsync(adminUser, adminPassword);
    if (result.Succeeded)
    {
        // Asignar el rol "Admin" al usuario
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Método seeder para localidades
async Task SeedLocalidadesAsync(ApplicationDbContext context)
{
    // Verificar si ya existen localidades
    if (context.Localidades.Any())
    {
        return; // Ya existen localidades, no crear más
    }

    var localidades = new[]
    {
        new Localidad { Nombre = "Las Varillas", Provincia = "Córdoba" },
        new Localidad { Nombre = "San Francisco", Provincia = "Córdoba" },
        new Localidad { Nombre = "Villa Maria", Provincia = "Córdoba" },
        new Localidad { Nombre = "Cordoba", Provincia = "Córdoba" },
        new Localidad { Nombre = "Arroyito", Provincia = "Córdoba" },
        new Localidad { Nombre = "Balnearia", Provincia = "Córdoba" },
        new Localidad { Nombre = "Santa Fe", Provincia = "Santa Fe" },
        new Localidad { Nombre = "Morteros", Provincia = "Córdoba" },
        new Localidad { Nombre = "Luque", Provincia = "Córdoba" },
        new Localidad { Nombre = "La Playosa", Provincia = "Córdoba" }
    };

    context.Localidades.AddRange(localidades);
    await context.SaveChangesAsync();
}
async Task SeedProductosAsync(ApplicationDbContext context)
{
    // Verificar si ya existen productos
    if (context.Productos.Any())
    {
        return;
    }

    var productos = new[]
    {
        new Producto
        {
            Nombre = "Cemento 50 kg",
            PrecioUnitario = 12500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Arena por m³",
            PrecioUnitario = 28000m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Piedra por m³",
            PrecioUnitario = 35000m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Ladrillo común",
            PrecioUnitario = 450m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Bloque de hormigón",
            PrecioUnitario = 1800m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Cal 25 kg",
            PrecioUnitario = 6500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Hierro 8 mm x 12 m",
            PrecioUnitario = 9500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Hierro 10 mm x 12 m",
            PrecioUnitario = 14500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Pintura látex 20 litros",
            PrecioUnitario = 42000m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Pegamento para cerámicos 30 kg",
            PrecioUnitario = 8500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Cerámico 50x50 cm",
            PrecioUnitario = 12500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Tornillos x 100 unidades",
            PrecioUnitario = 4500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Cable eléctrico 2,5 mm² x 100 m",
            PrecioUnitario = 38000m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Tomacorriente",
            PrecioUnitario = 3500m,
            Activo = true
        },

        new Producto
        {
            Nombre = "Mano de obra",
            PrecioUnitario = 25000m,
            Activo = true
        }
    };

    context.Productos.AddRange(productos);

    await context.SaveChangesAsync();
}


