using System.Security.Claims;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;
using Inmobiliaria1.Utils;          
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria1.Controllers;

public class UsuariosController : Controller
{
    private readonly IUsuarioRepository _repo;
    private readonly IWebHostEnvironment _env;

    public UsuariosController(IUsuarioRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    // ============================
    //        ABM (solo admin)

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    public async Task<IActionResult> Index(bool incluirInactivos = true)
    {
        var usuarios = await _repo.ListarAsync(incluirInactivos);
        return View(usuarios);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Usuario { Activo = true, Rol = RolUsuario.Empleado });
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Usuario u, IFormFile? avatarFile, string? password)
    {
        
        ModelState.Remove(nameof(Usuario.PasswordHash));

        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 6)
            ModelState.AddModelError("password", "La contraseña es obligatoria (mínimo 6 caracteres).");

        if (!ModelState.IsValid) return View(u);

        try
        {
            u.PasswordHash = PasswordHasher.Hash(password!.Trim());
            u.Avatar = null;
            u.Id = await _repo.AltaAsync(u);

            if (avatarFile is not null && avatarFile.Length > 0)
            {
                var rutaRel = await GuardarAvatarAsync(u.Id, avatarFile);
                await _repo.CambiarAvatarAsync(u.Id, rutaRel);
            }

            TempData["ok"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(Usuario.Email), "El email ya existe.");
            return View(u);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al crear usuario: " + ex.Message);
            return View(u);
        }
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador) )]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _repo.ObtenerPorIdAsync(id);
        if (u is null) return NotFound();
        return View(u);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador) )]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Usuario form, IFormFile? avatarFile)
    {
        if (id != form.Id) return BadRequest();

        // Evita que falte PasswordHash en el form te invalide el modelo
        ModelState.Remove(nameof(Usuario.PasswordHash));
        if (!ModelState.IsValid) return View(form);

        var u = await _repo.ObtenerPorIdAsync(id);
        if (u is null) return NotFound();

        try
        {
            // Campos editables por admin
            u.Email  = form.Email.Trim();
            u.Rol    = form.Rol;
            u.Activo = form.Activo;

            await _repo.ModificarAsync(u);

            if (avatarFile is not null && avatarFile.Length > 0)
            {
                var rutaRel = await GuardarAvatarAsync(u.Id, avatarFile);
                await _repo.CambiarAvatarAsync(u.Id, rutaRel);
            }

            TempData["ok"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627)
        {
            ModelState.AddModelError(nameof(Usuario.Email), "El email ya existe.");
            return View(form);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
            return View(form);
        }
    }

    // Cambiar la contraseña de OTRO usuario (solo admin)
    [Authorize(Roles = nameof(RolUsuario.Administrador) )]
    [HttpGet]
    public async Task<IActionResult> CambiarPassword(int id)
    {
        var u = await _repo.ObtenerPorIdAsync(id);
        if (u is null) return NotFound();
        ViewBag.UsuarioEmail = u.Email;
        return View(new CambiarPasswordVM { UsuarioId = id });
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador) )]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordVM vm)
    {
        if (string.IsNullOrWhiteSpace(vm.PasswordNueva) || vm.PasswordNueva.Trim().Length < 6)
        {
            ModelState.AddModelError(nameof(vm.PasswordNueva), "Mínimo 6 caracteres.");
            return View(vm);
        }

        var u = await _repo.ObtenerPorIdAsync(vm.UsuarioId);
        if (u is null) return NotFound();

        var nuevoHash = PasswordHasher.Hash(vm.PasswordNueva.Trim());
        await _repo.CmabiarPasswordAsync(vm.UsuarioId, nuevoHash);   // <- sin typo

        TempData["ok"] = "Contraseña actualizada.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var u = await _repo.ObtenerPorIdAsync(id);
        if (u is null) return NotFound();

        u.Activo = !u.Activo;
        await _repo.ModificarAsync(u);

        TempData["ok"] = $"Usuario {(u.Activo ? "activado" : "desactivado")}.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _repo.ObtenerPorIdAsync(id);
        if (u is null) return NotFound();
        return View(u);
    }

    [Authorize(Roles = nameof(RolUsuario.Administrador))]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.EliminarAsync(id);
        TempData["ok"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // ============================
    //        PERFIL PROPIO

    [Authorize]
    public async Task<IActionResult> MiPerfil()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var u = await _repo.ObtenerPorIdAsync(id);
        if (u is null) return NotFound();
        return View(u); // Views/Usuarios/MiPerfil.cshtml
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil(Usuario form, IFormFile? avatarFile)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id != form.Id) return Forbid();

        await _repo.ModificarPerfilAsync(new Usuario { Id = id, Email = form.Email.Trim() });

        if (avatarFile is not null && avatarFile.Length > 0)
        {
            var rutaRel = await GuardarAvatarAsync(id, avatarFile);
            await _repo.CambiarAvatarAsync(id, rutaRel);
        }

        TempData["ok"] = "Perfil actualizado.";
        return RedirectToAction(nameof(MiPerfil));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BorrarAvatar()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repo.CambiarAvatarAsync(id, null);
        TempData["ok"] = "Avatar eliminado.";
        return RedirectToAction(nameof(MiPerfil));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPasswordPropia(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 6)
        {
            TempData["err"] = "La contraseña debe tener al menos 6 caracteres.";
            return RedirectToAction(nameof(MiPerfil));
        }

        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hash = PasswordHasher.Hash(password.Trim());
        await _repo.CmabiarPasswordAsync(id, hash);                  // <- sin typo

        TempData["ok"] = "Contraseña actualizada.";
        return RedirectToAction(nameof(MiPerfil));
    }

    // ============================
    //           Helper
    // ============================

    private async Task<string> GuardarAvatarAsync(int userId, IFormFile file)
    {
        var dir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var permitido = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        if (!permitido.Contains(ext))
            throw new InvalidOperationException("Formato de imagen no permitido.");

        var nombre = $"{userId}{ext}";
        var rutaFisica = Path.Combine(dir, nombre);

        using (var fs = System.IO.File.Create(rutaFisica))
            await file.CopyToAsync(fs);

        return $"/avatars/{nombre}";
    }
}

// ViewModel (para admins cambiando la contraseña de otro usuario)
public class CambiarPasswordVM
{
    public int UsuarioId { get; set; }
    public string? PasswordNueva { get; set; }
}
