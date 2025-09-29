using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;

namespace Inmobiliaria1.Controllers;

[Authorize]
public class PagosController : Controller
{
    private readonly IPagoRepository _pagoRepo;
    private readonly IContratoRepository _ctrRepo;

    public PagosController(IPagoRepository pagoRepo, IContratoRepository ctrRepo)
    {
        _pagoRepo = pagoRepo;
        _ctrRepo  = ctrRepo;
    }

    public async Task<IActionResult> Index(int contratoId)
    {
        var contrato = await _ctrRepo.ObtenerPorIdAsync(contratoId);
        if (contrato is null) return NotFound();

        ViewBag.Contrato = contrato;
        ViewBag.Total    = await _pagoRepo.TotalPagadoAsync(contratoId);
        var data         = await _pagoRepo.ObtenerPorContratoAsync(contratoId);
        return View(data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _pagoRepo.ObtenerPorIdAsync(id);
        if (p is null) return NotFound();
        var c = await _ctrRepo.ObtenerPorIdAsync(p.ContratoId);
        ViewBag.Contrato = c;
        return View(p);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int contratoId)
    {
        var ctr = await _ctrRepo.ObtenerPorIdAsync(contratoId);
        if (ctr is null) return NotFound();
        return View(new Pago {
            ContratoId = contratoId,
            Fecha      = DateTime.Today,
            Importe    = ctr.MontoMensual,
            Estado     = EstadoPago.Activo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pago m)
    {
        ModelState.Remove(nameof(Pago.Numero));
        ModelState.Remove(nameof(Pago.CreadoPorUsuarioId));

        if (!ModelState.IsValid)
        {
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }

        try
        {
            m.CreadoPorUsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _pagoRepo.AltaAsync(m);
            TempData["ok"] = "Pago registrado.";
            return RedirectToAction(nameof(Index), new { contratoId = m.ContratoId });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            TempData["err"] = "El número de pago ya existe para este contrato.";
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }
        catch (Exception ex)
        {
            TempData["err"] = "Error al registrar el pago: " + ex.Message;
            ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(m.ContratoId);
            return View(m);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _pagoRepo.ObtenerPorIdAsync(id);
        if (p is null) return NotFound();
        if (p.Estado == EstadoPago.Anulado)
        {
            TempData["err"] = "No se puede editar un pago anulado.";
            return RedirectToAction(nameof(Index), new { contratoId = p.ContratoId });
        }
        ViewBag.Contrato = await _ctrRepo.ObtenerPorIdAsync(p.ContratoId);
        return View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int contratoId, string? detalle)
    {
        await _pagoRepo.EditarDetalleAsync(id, detalle);
        TempData["ok"] = "Detalle actualizado.";
        return RedirectToAction(nameof(Index), new { contratoId });
    }

    // Anular solo admin
    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var p = await _pagoRepo.ObtenerPorIdAsync(id);
        if (p is null) return NotFound();

        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _pagoRepo.AnularAsync(id, uid);

        TempData["ok"] = "Pago anulado.";
        return RedirectToAction(nameof(Index), new { contratoId = p.ContratoId });
    }
}

