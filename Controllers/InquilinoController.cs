using Microsoft.AspNetCore.Mvc;
using Inmobiliaria1.Models;
using Inmobiliaria1.Data.Repos;
using Microsoft.Data.SqlClient;

public class InquilinosController : Controller
{
    private readonly IInquilinoRepository _repo;
    public InquilinosController(IInquilinoRepository repo) => _repo = repo;

    public async Task<IActionResult> Index(string? q) => View(await _repo.ObtenerTodosAsync(q));

    public async Task<IActionResult> Details(int id)
    {
        var i = await _repo.ObtenerPorIdAsync(id);
        return i == null ? NotFound() : View(i);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inquilino i)
    {
        if (!ModelState.IsValid) return View(i);
        try { await _repo.AltaAsync(i); return RedirectToAction(nameof(Index)); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { ModelState.AddModelError("", "DNI o Email ya existe."); return View(i); }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var i = await _repo.ObtenerPorIdAsync(id);
        return i == null ? NotFound() : View(i);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inquilino i)
    {
        if (id != i.Id) return BadRequest();
        if (!ModelState.IsValid) return View(i);
        try { await _repo.ModificarAsync(i); return RedirectToAction(nameof(Index)); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { ModelState.AddModelError("", "DNI o Email ya existe."); return View(i); }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var i = await _repo.ObtenerPorIdAsync(id);
        return i == null ? NotFound() : View(i);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.BajaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
