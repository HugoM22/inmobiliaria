using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;

namespace Inmobiliaria1.Controllers;

public class PagosController : Controller
{
    private readonly IPagoRepository _pagoRepo;
    private readonly IContratoRepository _ctrRepo;

    public PagosController(IPagoRepository pagoRepo, IContratoRepository ctrRepo)
    {
        _pagoRepo = pagoRepo;
        _ctrRepo = ctrRepo;
    }

    public async Task<IActionResult> Index(int contratoId)
    {
        var contrato = await _ctrRepo.ObtenerPorIdAsync(contratoId);
        if (contrato == null) return NotFound();

        ViewBag.Contrato = contrato;
        var data = await _pagoRepo.ObtenerPorContratoAsync(contratoId);
        ViewBag.Total = await _pagoRepo.TotalPagadoAsync(contratoId);
        return View(data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var x = await _pagoRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        return View(x);
    }

    public async Task<IActionResult> Create(int contratoId)
    {
        var ctr = await _ctrRepo.ObtenerPorIdAsync(contratoId);
        if (ctr == null) return NotFound();

        var model = new Pago
        {
            ContratoId = contratoId,
            Fecha = DateTime.Today,
            Estado = EstadoPago.Activo,
            Numero = 1
        };

        ViewBag.Contrato = ctr;
        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pago m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }

        try
        {
            // TODO: reemplazar por usuario logueado
            m.CreadoPorUsuarioId = 1;
            await _pagoRepo.AltaAsync(m);
            return RedirectToAction(nameof(Index), new { contratoId = m.ContratoId });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(m.Numero), "Ese número de pago ya existe para este contrato.");
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var x = await _pagoRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(x.ContratoId);
        return View(x);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pago m)
    {
        if (id != m.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }

        try
        {
            await _pagoRepo.ModificarAsync(m);
            return RedirectToAction(nameof(Index), new { contratoId = m.ContratoId });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(m.Numero), "Ese número de pago ya existe para este contrato.");
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var x = await _pagoRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();
        ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(x.ContratoId);
        return View(x);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, int contratoId)
    {
        await _pagoRepo.BorrarAsync(id);
        return RedirectToAction(nameof(Index), new { contratoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var x = await _pagoRepo.ObtenerPorIdAsync(id);
        if (x == null) return NotFound();

        // TODO: reemplazar por usuario logueado
        await _pagoRepo.AnularAsync(id, 1);
        return RedirectToAction(nameof(Index), new { contratoId = x.ContratoId });
    }
}
