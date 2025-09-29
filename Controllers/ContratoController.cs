using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;

namespace Inmobiliaria1.Controllers;

[Authorize]
public class ContratosController : Controller
{
    private readonly IContratoRepository _ctrRepo;
    private readonly IInmuebleRepository _inmRepo;
    private readonly IInquilinoRepository _inqRepo;

    public ContratosController(
        IContratoRepository ctrRepo,
        IInmuebleRepository inmRepo,
        IInquilinoRepository inqRepo)
    {
        _ctrRepo = ctrRepo;
        _inmRepo = inmRepo;
        _inqRepo = inqRepo;
    }

    private async Task CargarCombosAsync(int? inmuebleSel = null, int? inquilinoSel = null)
    {
        // Inmuebles: sólo “disponibles” por estado (Publicado)
        var inmuebles = await _inmRepo.ListarDisponiblesAsync();
        ViewBag.InmuebleId = new SelectList(inmuebles, "Id", "Direccion", inmuebleSel);

        // Inquilinos: mostrar “Apellido Nombre”
        var inquilinos = await _inqRepo.ObtenerTodosAsync();
        ViewBag.InquilinoId = new SelectList(
            inquilinos.Select(x => new { x.Id, Nombre = x.Apellido + " " + x.Nombre }),
            "Id", "Nombre", inquilinoSel);
    }

    // -------- Index / Details --------
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

    // -------- Create --------
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarCombosAsync();
        var model = new Contrato
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddMonths(12),
            Estado = EstadoContrato.Vigente
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contrato m)
    {
        ModelState.Remove(nameof(Contrato.CreadoPorUsuarioId));

        if (m.FechaFin <= m.FechaInicio)
            ModelState.AddModelError(nameof(m.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");

        var inm = await _inmRepo.ObtenerPorIdAsync(m.InmuebleId);
        if (inm is null)
            ModelState.AddModelError(nameof(m.InmuebleId), "El inmueble seleccionado no existe.");
        else
            m.MontoMensual = inm.Precio;

        if (await _ctrRepo.ExisteSolapamientoAsync(m.InmuebleId, m.FechaInicio, m.FechaFin))
            ModelState.AddModelError(string.Empty, "El inmueble está ocupado en esas fechas.");

        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(m.InmuebleId, m.InquilinoId);
            return View(m);
        }

        m.CreadoPorUsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        m.FechaFinEfectiva = null;
        m.FinalizadoPorUsuarioId = null;

        await _ctrRepo.AltaAsync(m);
        TempData["ok"] = "Contrato creado.";
        return RedirectToAction(nameof(Index));
    }

    // -------- Edit --------
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var x = await _ctrRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        await CargarCombosAsync(x.InmuebleId, x.InquilinoId);
        return View(x);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contrato m)
    {
        if (id != m.Id) return NotFound();
        ModelState.Remove(nameof(Contrato.CreadoPorUsuarioId));

        if (m.FechaFin <= m.FechaInicio)
            ModelState.AddModelError(nameof(m.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");

        if (await _ctrRepo.ExisteSolapamientoAsync(m.InmuebleId, m.FechaInicio, m.FechaFin, m.Id))
            ModelState.AddModelError(string.Empty, "El inmueble está ocupado en esas fechas.");

        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(m.InmuebleId, m.InquilinoId);
            return View(m);
        }

        await _ctrRepo.ModificarAsync(m);
        TempData["ok"] = "Contrato actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // -------- Delete --------
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await _ctrRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        return View(x);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _ctrRepo.BorrarAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // -------- Finalizar--------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int id, DateTime fechaFinEfectiva)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _ctrRepo.FinalizarAsync(id, userId, fechaFinEfectiva, EstadoContrato.Finalizado);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> PrecioInmueble(int id)
    {
        var i = await _inmRepo.ObtenerPorIdAsync(id);
        if (i is null) return NotFound();
        return Json(new { precio = i.Precio });
    }
}

