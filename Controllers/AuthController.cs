using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria1.Data.Repos;
using Inmobiliaria1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


public class AuthController : Controller
{
    private readonly IUsuarioRepository _repo;
    public AuthController(IUsuarioRepository repo)
    {
        _repo = repo;
    }
    [HttpGet,AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    [HttpPost, ValidateAntiForgeryToken,AllowAnonymous]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
{
    var u = await _repo.ObtenerPorEmailAsync(email);

    if (u is null || !u.Activo)
    {
        ModelState.AddModelError("", "Email o contraseña incorrecta");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    var idHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
    bool ok;

    try
    {
        ok = idHasher.VerifyHashedPassword(u, u.PasswordHash, password)
             != PasswordVerificationResult.Failed;
    }
    catch (FormatException)
    {
        ok = Inmobiliaria1.Utils.PasswordHasher.Verify(password, u.PasswordHash);
        if (ok)
        {
            u.PasswordHash = idHasher.HashPassword(u, password);
            await _repo.CmabiarPasswordAsync(u.Id, u.PasswordHash);
        }
    }

    if (!ok)
    {
        ModelState.AddModelError("", "Email o contraseña incorrecta");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
        new Claim(ClaimTypes.Email, u.Email),
        new Claim(ClaimTypes.Role, u.Rol.ToString())
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));

    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);

    return RedirectToAction("Index", "Home");
}
    [HttpPost,ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}