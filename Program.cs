using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

using Inmobiliaria1.Data;
using Inmobiliaria1.Data.Repos;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// ---- Localización es-AR
var supportedCultures = new[] { new CultureInfo("es-AR") };
builder.Services.Configure<RequestLocalizationOptions>(o =>
{
    o.DefaultRequestCulture = new RequestCulture("es-AR");
    o.SupportedCultures = supportedCultures;
    o.SupportedUICultures = supportedCultures;
});

// Repos
builder.Services.AddScoped<IPropietarioRepository, PropietarioRepository>();
builder.Services.AddScoped<IInquilinoRepository,  InquilinoRepository>();
builder.Services.AddScoped<IInmuebleRepository,   InmuebleRepository>();
builder.Services.AddScoped<IContratoRepository,   ContratoRepository>();
builder.Services.AddScoped<IPagoRepository,       PagoRepository>();
builder.Services.AddScoped<ITipoInmuebleRepository, TipoInmuebleRepository>();

var app = builder.Build();

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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


