using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Ejecutar seeder de roles y usuarios
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    await SeedRolesAsync(roleManager);
    await SeedUsersAsync(userManager);
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

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
