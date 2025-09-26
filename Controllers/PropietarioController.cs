using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Models;
using Inmobiliaria1.Data.Repos;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria1.Controllers;

[Authorize]
public class PropietariosController : Controller
{
    private readonly IPropietarioRepository _repo;
    public PropietariosController(IPropietarioRepository repo) => _repo = repo;

    public async Task<IActionResult> Index(string? q) =>
        View(await _repo.ObtenerTodosAsync(q));

    public async Task<IActionResult> Details(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? NotFound() : View(p);
    }

    public IActionResult Create() => View();


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Propietario p)
    {
        if (!ModelState.IsValid) return View(p);
        var nombreOriginal = p.Nombre;
        try
        {
            await _repo.AltaAsync(p);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { ModelState.AddModelError("", "DNI o Email ya existe."); return View(p); }
    }


    public async Task<IActionResult> Edit(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? NotFound() : View(p);
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Propietario p)
    {
        if (id != p.Id) return BadRequest();
        if (!ModelState.IsValid) return View(p);
        try { await _repo.ModificarAsync(p); return RedirectToAction(nameof(Index)); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { ModelState.AddModelError("", "DNI o Email ya existe."); return View(p); }
    }


[Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? NotFound() : View(p);
    }


    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.BajaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
