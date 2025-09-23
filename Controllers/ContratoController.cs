using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;

namespace Inmobiliaria1.Controllers;

public class ContratosController : Controller
{
    private readonly IContratoRepository _ctrRepo;
    private readonly IInmuebleRepository _inmRepo;
    private readonly IInquilinoRepository _inqRepo;

    public ContratosController(IContratoRepository ctrRepo, IInmuebleRepository inmRepo, IInquilinoRepository inqRepo)
    {
        _ctrRepo = ctrRepo;
        _inmRepo = inmRepo;
        _inqRepo = inqRepo;
    }

    // GET: Contratos
    public async Task<IActionResult> Index(int? inmuebleId, int? inquilinoId, string? estado)
    {
        var data = await _ctrRepo.ObtenerTodosAsync(estado, inquilinoId, inmuebleId);
        ViewBag.InmuebleId = inmuebleId;
        ViewBag.InquilinoId = inquilinoId;
        ViewBag.Estado = estado;
        return View(data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var x = await _ctrRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        return View(x);
    }

    private async Task CargarCombosAsync(int? inmuebleSel = null, int? inquilinoSel = null)
    {
        // Inmuebles
        var inmuebles = await _inmRepo.ObtenerTodosAsync();
        ViewBag.InmuebleId = new SelectList(inmuebles, "Id", "Direccion", inmuebleSel);

        // Inquilinos
        var inquilinos = await _inqRepo.ObtenerTodosAsync();
        ViewBag.InquilinoId = new SelectList(inquilinos, "Id", "Apellido", inquilinoSel);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCombosAsync();
        return View(new Contrato {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddMonths(12),
            Estado = EstadoContrato.Vigente
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contrato m)
    {
        if (m.FechaFin <= m.FechaInicio)
            ModelState.AddModelError(nameof(m.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");

        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(m.InmuebleId, m.InquilinoId);
            return View(m);
        }

        try
        {
            // TODO: reemplazar 1 por el usuario logueado
            m.CreadoPorUsuarioId = 1;
            await _ctrRepo.AltaAsync(m);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("", "Ya existe un registro con esos datos únicos.");
            await CargarCombosAsync(m.InmuebleId, m.InquilinoId);
            return View(m);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var x = await _ctrRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        await CargarCombosAsync(x.InmuebleId, x.InquilinoId);
        return View(x);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contrato m)
    {
        if (id != m.Id) return NotFound();
        if (m.FechaFin <= m.FechaInicio)
            ModelState.AddModelError(nameof(m.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");

        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(m.InmuebleId, m.InquilinoId);
            return View(m);
        }

        try
        {
            await _ctrRepo.ModificarAsync(m);
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError("", "Ya existe un registro con esos datos únicos.");
            await CargarCombosAsync(m.InmuebleId, m.InquilinoId);
            return View(m);
        }
    }

    
    public async Task<IActionResult> Delete(int id)
    {
        var x = await _ctrRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        return View(x);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _ctrRepo.BorrarAsync(id);
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int id, DateTime fechaFinEfectiva)
    {
        // TODO: reemplazar por usuario logueado
        var userId = 1;
        await _ctrRepo.FinalizarAsync(id, userId, fechaFinEfectiva, EstadoContrato.Finalizado);
        return RedirectToAction(nameof(Details), new { id });
    }
}
