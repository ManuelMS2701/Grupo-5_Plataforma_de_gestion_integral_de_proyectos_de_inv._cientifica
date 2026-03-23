using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class InstitucionesController : Controller
    {
        private readonly ResearchHubContext _context;

        public InstitucionesController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var instituciones = await _context.Instituciones
                .AsNoTracking()
                .ToListAsync();

            return View(instituciones);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var institucion = await _context.Instituciones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdInstitucion == id.Value);

            if (institucion == null) return NotFound();

            return View(institucion);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Institucion institucion)
        {
            if (!ModelState.IsValid)
            {
                return View(institucion);
            }

            _context.Instituciones.Add(institucion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var institucion = await _context.Instituciones.FindAsync(id.Value);
            if (institucion == null) return NotFound();

            return View(institucion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Institucion institucion)
        {
            if (id != institucion.IdInstitucion) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(institucion);
            }

            try
            {
                _context.Update(institucion);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await InstitucionExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var institucion = await _context.Instituciones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdInstitucion == id.Value);

            if (institucion == null) return NotFound();

            return View(institucion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var institucion = await _context.Instituciones.FindAsync(id);
            if (institucion == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var tieneInvestigadores = await _context.Investigadores
                .AsNoTracking()
                .AnyAsync(i => i.IdInstitucion == id);

            if (tieneInvestigadores)
            {
                TempData["DeleteError"] = "No se puede eliminar: hay investigadores asociados a esta institución.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Instituciones.Remove(institucion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> InstitucionExists(int id)
        {
            return await _context.Instituciones.AnyAsync(e => e.IdInstitucion == id);
        }
    }
}
