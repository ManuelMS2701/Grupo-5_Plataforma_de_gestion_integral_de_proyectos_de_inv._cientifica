using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private const int MaxIntentos = 5;
        private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

        private readonly ResearchHubContext _context;
        private readonly PasswordHasher<UsuarioSistema> _passwordHasher = new();

        public AccountController(ResearchHubContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var criterio = model.Email.Trim().ToLowerInvariant();
            var usuario = await _context.UsuariosSistema
                .Include(u => u.Rol)
                .Include(u => u.Investigador)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == criterio || u.NombreUsuario.ToLower() == criterio);

            if (usuario == null || !usuario.Activo)
            {
                ModelState.AddModelError(string.Empty, "Credenciales invalidas.");
                return View(model);
            }

            if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada por exceso de intentos. Intenta nuevamente en 15 minutos.");
                return View(model);
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password);
            if (resultado == PasswordVerificationResult.Failed)
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= MaxIntentos)
                {
                    usuario.BloqueadoHasta = DateTime.UtcNow.Add(DuracionBloqueo);
                    usuario.IntentosFallidos = 0;
                }

                await _context.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "Credenciales invalidas.");
                return View(model);
            }

            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(BuildClaims(usuario), CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(10)
                });

            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewData["Roles"] = await BuildRolesSelectAsync(permitirRolesPrivilegiados: false);
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await BuildRolesSelectAsync(model.IdRol, permitirRolesPrivilegiados: false);
                return View(model);
            }

            var email = model.Email.Trim().ToLowerInvariant();
            var nombreUsuario = model.NombreUsuario.Trim().ToLowerInvariant();

            if (await _context.UsuariosSistema.AnyAsync(u => u.Email.ToLower() == email))
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese correo.");
            }

            if (await _context.UsuariosSistema.AnyAsync(u => u.NombreUsuario.ToLower() == nombreUsuario))
            {
                ModelState.AddModelError(nameof(model.NombreUsuario), "Ya existe un usuario con ese nombre de usuario.");
            }

            var rol = await _context.RolesSistema.FirstOrDefaultAsync(r =>
                r.IdRol == model.IdRol &&
                r.Activo &&
                Roles.RolesRegistroPublico.Contains(r.Nombre));

            if (rol == null)
            {
                ModelState.AddModelError(nameof(model.IdRol), "Selecciona un rol valido para registro publico.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await BuildRolesSelectAsync(model.IdRol, permitirRolesPrivilegiados: false);
                return View(model);
            }

            var investigador = await _context.Investigadores
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Email.ToLower() == email);

            var usuario = new UsuarioSistema
            {
                NombreCompleto = model.NombreCompleto.Trim(),
                NombreUsuario = model.NombreUsuario.Trim(),
                Email = model.Email.Trim(),
                IdRol = model.IdRol,
                IdInvestigador = investigador?.IdInvestigador,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password);

            _context.UsuariosSistema.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private IEnumerable<Claim> BuildClaims(UsuarioSistema usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("display_name", usuario.NombreCompleto),
                new Claim("username", usuario.NombreUsuario)
            };

            if (usuario.Rol != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.Nombre));
            }

            if (usuario.IdInvestigador.HasValue)
            {
                claims.Add(new Claim("investigador_id", usuario.IdInvestigador.Value.ToString()));
            }

            return claims;
        }

        private async Task<Microsoft.AspNetCore.Mvc.Rendering.SelectList> BuildRolesSelectAsync(int? idRol = null, bool permitirRolesPrivilegiados = true)
        {
            var query = _context.RolesSistema
                .AsNoTracking()
                .Where(r => r.Activo)
                .AsQueryable();

            if (!permitirRolesPrivilegiados)
            {
                query = query.Where(r => Roles.RolesRegistroPublico.Contains(r.Nombre));
            }

            var roles = await query
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return new Microsoft.AspNetCore.Mvc.Rendering.SelectList(roles, "IdRol", "Nombre", idRol);
        }
    }
}
