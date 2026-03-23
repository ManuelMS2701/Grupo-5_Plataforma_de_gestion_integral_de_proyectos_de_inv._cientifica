using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize]
    public class ProyectosController : Controller
    {
        private readonly ResearchHubContext _context;

        public ProyectosController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var proyectos = await _context.Proyectos
                .Include(p => p.Institucion)
                .AsNoTracking()
                .ToListAsync();

            return View(proyectos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos
                .Include(p => p.Institucion)
                .Include(p => p.InvestigadorPrincipal)
                .Include(p => p.LineaInvestigacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProyecto == id.Value);

            if (proyecto == null) return NotFound();

            return View(proyecto);
        }

        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Create(Proyecto proyecto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync();
                return View(proyecto);
            }

            proyecto.FechaCreacion = DateTime.UtcNow;
            _context.Proyectos.Add(proyecto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos.FindAsync(id.Value);
            if (proyecto == null) return NotFound();

            await CargarCombosAsync();
            return View(proyecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Edit(int id, Proyecto proyecto)
        {
            if (id != proyecto.IdProyecto) return NotFound();

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync();
                return View(proyecto);
            }

            try
            {
                _context.Update(proyecto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProyectoExisteAsync(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos
                .Include(p => p.Institucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProyecto == id.Value);

            if (proyecto == null) return NotFound();

            return View(proyecto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombosAsync()
        {
            ViewData["Investigadores"] = new SelectList(
                await _context.Investigadores.AsNoTracking().ToListAsync(),
                "IdInvestigador",
                "Nombre");

            ViewData["Instituciones"] = new SelectList(
                await _context.Instituciones.AsNoTracking().ToListAsync(),
                "IdInstitucion",
                "Nombre");

            ViewData["Lineas"] = new SelectList(
                await _context.LineasInvestigacion.AsNoTracking().ToListAsync(),
                "IdLinea",
                "Nombre");
        }

        private async Task<bool> ProyectoExisteAsync(int id)
        {
            return await _context.Proyectos.AnyAsync(e => e.IdProyecto == id);
        }
    }
}
