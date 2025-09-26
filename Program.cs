using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;               

using Inmobiliaria1.Data;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Localización es-AR
var supportedCultures = new[] { new CultureInfo("es-AR") };
builder.Services.Configure<RequestLocalizationOptions>(o =>
{
    o.DefaultRequestCulture = new RequestCulture("es-AR");
    o.SupportedCultures = supportedCultures;
    o.SupportedUICultures = supportedCultures;
});

// Auth: Cookies
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";       
        o.AccessDeniedPath = "/Auth/Denegado"; // vista para 403;
    });

builder.Services.AddAuthorization(o =>
{
    // Política de ejemplo para admins
    o.AddPolicy("SoloAdmin", p =>
        p.RequireRole(nameof(RolUsuario.Administrador)));
});

// repos
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPropietarioRepository, PropietarioRepository>();
builder.Services.AddScoped<IInquilinoRepository,  InquilinoRepository>();
builder.Services.AddScoped<IInmuebleRepository,   InmuebleRepository>();
builder.Services.AddScoped<IContratoRepository,   ContratoRepository>();
builder.Services.AddScoped<IPagoRepository,       PagoRepository>();
builder.Services.AddScoped<ITipoInmuebleRepository, TipoInmuebleRepository>();

Console.WriteLine("CS: " + builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();


// ======crea admin por única vez ======
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

    var seedEmail = "Admin@gmail.com";
    var seedPass  = "Admin123!";

    var existing = await repo.ObtenerPorEmailAsync(seedEmail);
    if (existing == null)
    {
        var hasher = new PasswordHasher<Usuario>();
        var admin = new Usuario
        {
            Email = seedEmail,
            Rol   = RolUsuario.Administrador,
            Activo = true,
            PasswordHash = ""
        };
        admin.PasswordHash = hasher.HashPassword(admin, seedPass);
        await repo.AltaAsync(admin);
    }
}
// ====== fin ======

// ======crea Empleado por única vez ======
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

   
    var seedEmail = "Empleado@gmail.com";
    var seedPass  = "Empleado123!";

    var existing = await repo.ObtenerPorEmailAsync(seedEmail);
    if (existing == null)
    {
        var hasher = new PasswordHasher<Usuario>();
        var admin = new Usuario
        {
            Email = seedEmail,
            Rol   = RolUsuario.Empleado,
            Activo = true,
            PasswordHash = ""
        };
        admin.PasswordHash = hasher.HashPassword(admin, seedPass);
        await repo.AltaAsync(admin);
    }
}
// ====== fin ======


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// activa la cultura configurada arriba
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseRouting();

// **Importante: auth antes de authorization**
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();




