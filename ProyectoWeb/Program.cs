using Aplicacion.Application.Services;
using Aplicacion.Core.Configuration;
using Aplicacion.Core.Interfaces;
using DeluxeCars.DataAccess;
using DeluxeCars.DataAccess.Repositories.Implementations;
using DeluxeCars.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE LA NUEVA ARQUITECTURA ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 1. Usamos el AppDbContext centralizado de DeluxeCars.DataAccess
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Usamos IUnitOfWork como la forma de acceder a los datos.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- FUNCIONALIDADES DEL ARCHIVO VIEJO ---

// <-- AÑADIDO: Configuración para el envío de correos desde secrets.json. -->
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// <-- AÑADIDO: Servicio para el carrito de compras. -->
// Nota: Asegúrate que la clase CarritoService no dependa de los servicios viejos, sino que pueda funcionar con la nueva arquitectura.
builder.Services.AddScoped<ICarritoService, CarritoService>();

builder.Services.AddScoped<IContentService, ContentService>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// <-- AÑADIDO: Configuración para la sesión de usuario, necesaria para el carrito. -->
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- CONFIGURACIÓN DE IDENTITY Y COOKIES (Común a ambos archivos) ---
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    options.LoginPath = "/Account/Login"; // redirige aqui si no esta autenticado
    options.LogoutPath = "/Account/Logout"; // Al cerrar la sesion
    options.AccessDeniedPath = "/Account/AccessDenied"; // Si no tiene permisos
    options.SlidingExpiration = true; // Renueva el tiempo de sesion con actividad

    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// --- CONFIGURACIÓN DEL PIPELINE HTTP ---
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

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

async Task CreateRoles(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Administrador", "Usuario" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

async Task CreateUsers(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Crear usuario administrador (Johan Suarez)
    string adminEmail = "johan.suarez@autopartes.com";
    string adminPassword = "Admin123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(newAdmin, adminPassword);
        await userManager.AddToRoleAsync(newAdmin, "Administrador");
    }

    // Crear usuario cliente (Karen Briceño)
    string userEmail = "karen.briceno@autopartes.com";
    string userPassword = "User123!";
    var regularUser = await userManager.FindByEmailAsync(userEmail);
    if (regularUser == null)
    {
        var newUser = new IdentityUser
        {
            UserName = userEmail,
            Email = userEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(newUser, userPassword);
        await userManager.AddToRoleAsync(newUser, "Usuario");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services);
    await CreateUsers(services);
}

app.Run();
