using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class UsuariosSistemaController : Controller
    {
        private readonly ResearchHubContext _context;
        private readonly PasswordHasher<UsuarioSistema> _passwordHasher = new();

        public UsuariosSistemaController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.UsuariosSistema
                .Include(u => u.Rol)
                .Include(u => u.Investigador)
                .AsNoTracking()
                .OrderBy(u => u.NombreCompleto);
            var totalItems = await query.CountAsync();
            var usuarios = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(usuarios);
        }


        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new UsuarioSistemaFormViewModel { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioSistemaFormViewModel model)
        {
            await ValidarUsuarioAsync(model);
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "La contraseña es obligatoria.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(model.IdRol, model.IdInvestigador);
                return View(model);
            }

            var usuario = new UsuarioSistema
            {
                NombreCompleto = model.NombreCompleto.Trim(),
                NombreUsuario = model.NombreUsuario.Trim(),
                Email = model.Email.Trim(),
                IdRol = model.IdRol,
                IdInvestigador = model.IdInvestigador,
                Activo = model.Activo,
                FechaRegistro = DateTime.UtcNow
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password!);
            _context.UsuariosSistema.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(id);
            if (usuario == null) return NotFound();

            await CargarCombosAsync(usuario.IdRol, usuario.IdInvestigador);
            return View(new UsuarioSistemaFormViewModel
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = usuario.NombreCompleto,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                IdRol = usuario.IdRol,
                IdInvestigador = usuario.IdInvestigador,
                Activo = usuario.Activo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioSistemaFormViewModel model)
        {
            if (id != model.IdUsuario) return NotFound();

            var usuario = await _context.UsuariosSistema.FindAsync(id);
            if (usuario == null) return NotFound();

            await ValidarUsuarioAsync(model, id);
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(model.IdRol, model.IdInvestigador);
                return View(model);
            }

            usuario.NombreCompleto = model.NombreCompleto.Trim();
            usuario.NombreUsuario = model.NombreUsuario.Trim();
            usuario.Email = model.Email.Trim();
            usuario.IdRol = model.IdRol;
            usuario.IdInvestigador = model.IdInvestigador;
            usuario.Activo = model.Activo;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password);
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.UsuariosSistema
                .Include(u => u.Rol)
                .Include(u => u.Investigador)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.UsuariosSistema.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var usuario = await _context.UsuariosSistema.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Activo = !usuario.Activo;
            if (usuario.Activo)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidarUsuarioAsync(UsuarioSistemaFormViewModel model, int? idActual = null)
        {
            if (await _context.UsuariosSistema.AnyAsync(u => u.IdUsuario != idActual && u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese correo.");
            }

            if (await _context.UsuariosSistema.AnyAsync(u => u.IdUsuario != idActual && u.NombreUsuario == model.NombreUsuario))
            {
                ModelState.AddModelError(nameof(model.NombreUsuario), "Ya existe un usuario con ese nombre de usuario.");
            }
        }

        private async Task CargarCombosAsync(int? idRol = null, int? idInvestigador = null)
        {
            ViewData["Roles"] = new SelectList(
                await _context.RolesSistema
                    .AsNoTracking()
                    .Where(r => r.Activo)
                    .OrderBy(r => r.Nombre)
                    .ToListAsync(),
                "IdRol",
                "Nombre",
                idRol);

            ViewData["Investigadores"] = new SelectList(
                await _context.Investigadores
                    .AsNoTracking()
                    .OrderBy(i => i.Apellido)
                    .ThenBy(i => i.Nombre)
                    .Select(i => new
                    {
                        i.IdInvestigador,
                        Nombre = i.Nombre + " " + i.Apellido + " · " + i.Email
                    })
                    .ToListAsync(),
                "IdInvestigador",
                "Nombre",
                idInvestigador);
        }
    }
}
