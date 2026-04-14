using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class RolesSistemaController : Controller
    {
        private readonly ResearchHubContext _context;

        public RolesSistemaController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.RolesSistema
                .Include(r => r.Usuarios)
                .AsNoTracking()
                .OrderBy(r => r.Nombre);
            var totalItems = await query.CountAsync();
            var roles = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(roles);
        }


        public IActionResult Create()
        {
            return View(new RolSistema { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolSistema rol)
        {
            if (await _context.RolesSistema.AnyAsync(r => r.Nombre == rol.Nombre))
            {
                ModelState.AddModelError(nameof(RolSistema.Nombre), "Ya existe un rol con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                return View(rol);
            }

            _context.RolesSistema.Add(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var rol = await _context.RolesSistema.FindAsync(id);
            if (rol == null) return NotFound();
            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RolSistema rol)
        {
            if (id != rol.IdRol) return NotFound();

            if (await _context.RolesSistema.AnyAsync(r => r.IdRol != id && r.Nombre == rol.Nombre))
            {
                ModelState.AddModelError(nameof(RolSistema.Nombre), "Ya existe un rol con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                return View(rol);
            }

            _context.Update(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var rol = await _context.RolesSistema
                .Include(r => r.Usuarios)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRol == id);
            if (rol == null) return NotFound();
            return View(rol);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rol = await _context.RolesSistema
                .Include(r => r.Usuarios)
                .FirstOrDefaultAsync(r => r.IdRol == id);
            if (rol == null) return NotFound();

            if (rol.Usuarios.Any())
            {
                TempData["Error"] = "No puedes eliminar un rol que tiene usuarios asignados.";
                return RedirectToAction(nameof(Index));
            }

            _context.RolesSistema.Remove(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
