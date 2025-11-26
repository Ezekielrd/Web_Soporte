using DGASoporte.Data;
using DGASoporte.Hubs;
using DGASoporte.Infraestructura;
using DGASoporte.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AsignacionTareasService>();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<DGADbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
// Servicios propios
builder.Services.AddScoped<IAutentificacionServicio, AutentificacionServicio>();
builder.Services.AddControllersWithViews();

//el mecanismo principal para manejar sesiones de usuarios será la autenticación basada en cookies.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Cuenta/Login";         //Ruta a la que se redirige si el usuario no está autenticado
        opt.LogoutPath = "/Cuenta/Logout";      // Ruta para cerrar sesión
        opt.AccessDeniedPath = "/Cuenta/Denied";// Ruta para acceso denegado
        opt.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo de expiración de la cookie
        opt.SlidingExpiration = true;   // Renueva el tiempo de expiración si el usuario está activo
    });
builder.Services.AddAuthorization();

var app = builder.Build();
// Inicializa BD + Seed
await Iniciar.InitializeAsync(app.Services);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapHub<NotificacionesHub>("/hubs/notificaciones");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
