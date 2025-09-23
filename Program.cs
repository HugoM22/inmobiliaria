using Inmobiliaria1.Data;
using Inmobiliaria1.Data.Repos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core con SQL Server (LocalDB)
builder.Services.AddScoped<IPropietarioRepository, PropietarioRepository>();
builder.Services.AddScoped<IInquilinoRepository,  InquilinoRepository>();
builder.Services.AddScoped<IInmuebleRepository,   InmuebleRepository>();
builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<ITipoInmuebleRepository, TipoInmuebleRepository>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();              
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

