using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria1.Controllers;

[Authorize]
public class InmueblesController : Controller
{
    private readonly IInmuebleRepository _inmRepo;
    private readonly IPropietarioRepository _propRepo;
    private readonly ITipoInmuebleRepository _tipoRepo;

    public InmueblesController(IInmuebleRepository inmRepo, IPropietarioRepository propRepo, ITipoInmuebleRepository tipoRepo)
    {
        _inmRepo = inmRepo;
        _propRepo = propRepo;
        _tipoRepo = tipoRepo;
    }

    public async Task<IActionResult> Index(string? q, int? tipoId, int? propietarioId)
    {
        var data = await _inmRepo.ObtenerTodosAsync(q, propietarioId, tipoId);
        ViewBag.Query = q;
        ViewBag.TipoId = tipoId;
        ViewBag.PropietarioId = propietarioId;
        return View(data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var x = await _inmRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        return View(x);
    }

    private async Task CargarCombosAsync(int? tipoSel = null, int? propSel = null)
    {
        var tipos = await _tipoRepo.ObtenerTodosAsync();
        ViewBag.TipoInmuebleId = new SelectList(tipos, "Id", "Descripcion", tipoSel);

        var props = await _propRepo.ObtenerTodosAsync();
        ViewBag.PropietarioId = new SelectList(props, "Id", "Apellido", propSel);
    }


    public async Task<IActionResult> Create()
    {
        await CargarCombosAsync();
        return View(new Inmueble { Uso = Uso.Residencial, Estado = EstadoInmueble.Publicado, Ambientes = 1 });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inmueble m)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(m.TipoInmuebleId, m.PropietarioId);
            return View(m);
        }
        var direccionOriginal = m.Direccion;
        try
        {
            await _inmRepo.AltaAsync(m);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("", "Ya existe un registro con esos datos únicos.");
            await CargarCombosAsync(m.TipoInmuebleId, m.PropietarioId);
            return View(m);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var x = await _inmRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        await CargarCombosAsync(x.TipoInmuebleId, x.PropietarioId);
        return View(x);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inmueble m)
    {
        if (id != m.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(m.TipoInmuebleId, m.PropietarioId);
            return View(m);
        }

        try
        {
            await _inmRepo.ModificarAsync(m);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("", "Ya existe un registro con esos datos únicos.");
            await CargarCombosAsync(m.TipoInmuebleId, m.PropietarioId);
            return View(m);
        }
    }


    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await _inmRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        return View(x);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _inmRepo.BajaAsync(id);
        return RedirectToAction(nameof(Index));
    }
    [Authorize]
    public async Task<IActionResult> Disponibles()
    {
        var data = await _inmRepo.ListarDisponiblesAsync();
        return View(data);
    }
    [HttpGet]
public async Task<IActionResult> Precio(int id)
{
    var i = await _inmRepo.ObtenerPorIdAsync(id);
    if (i is null) return NotFound();
    return Content(i.Precio.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
}
