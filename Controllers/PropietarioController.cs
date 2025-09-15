using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Models;
using Inmobiliaria1.Data.Repos;

namespace Inmobiliaria1.Controllers;

public class PropietariosController : Controller
{
    private readonly IPropietarioRepository _repo;
    public PropietariosController(IPropietarioRepository repo) => _repo = repo;

    // GET: /Propietarios
    public async Task<IActionResult> Index(string? q) =>
        View(await _repo.ObtenerTodosAsync(q));

    // GET: /Propietarios/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? NotFound() : View(p);
    }

    // GET: /Propietarios/Create
    public IActionResult Create() => View();

    // POST: /Propietarios/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Propietario p)
    {
        if (!ModelState.IsValid) return View(p);
        try { await _repo.AltaAsync(p); return RedirectToAction(nameof(Index)); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { ModelState.AddModelError("", "DNI o Email ya existe."); return View(p); }
    }

    // GET: /Propietarios/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? NotFound() : View(p);
    }

    // POST: /Propietarios/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Propietario p)
    {
        if (id != p.Id) return BadRequest();
        if (!ModelState.IsValid) return View(p);
        try { await _repo.ModificarAsync(p); return RedirectToAction(nameof(Index)); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { ModelState.AddModelError("", "DNI o Email ya existe."); return View(p); }
    }

    // GET: /Propietarios/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? NotFound() : View(p);
    }

    // POST: /Propietarios/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.BajaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
