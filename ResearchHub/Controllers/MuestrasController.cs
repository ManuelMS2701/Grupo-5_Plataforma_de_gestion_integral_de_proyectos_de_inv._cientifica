using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class MuestrasController : Controller
    {
        private readonly ResearchHubContext _context;

        public MuestrasController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.Muestras
                .Include(m => m.Proyecto)
                .AsNoTracking()
                .OrderByDescending(m => m.FechaRecoleccion);
            var totalItems = await query.CountAsync();
            var muestras = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(muestras);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var muestra = await _context.Muestras
                .Include(m => m.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdMuestra == id.Value);

            if (muestra == null) return NotFound();

            return View(muestra);
        }

        public async Task<IActionResult> Create()
        {
            await CargarProyectosAsync();
            return View(new Muestra());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Muestra muestra)
        {
            if (!ModelState.IsValid)
            {
                await CargarProyectosAsync();
                return View(muestra);
            }

            _context.Muestras.Add(muestra);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var muestra = await _context.Muestras.FindAsync(id.Value);
            if (muestra == null) return NotFound();

            await CargarProyectosAsync();
            return View(muestra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Muestra muestra)
        {
            if (id != muestra.IdMuestra) return NotFound();

            if (!ModelState.IsValid)
            {
                await CargarProyectosAsync();
                return View(muestra);
            }

            try
            {
                _context.Update(muestra);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExisteAsync(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var muestra = await _context.Muestras
                .Include(m => m.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdMuestra == id.Value);

            if (muestra == null) return NotFound();

            return View(muestra);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var muestra = await _context.Muestras.FindAsync(id);
            if (muestra != null)
            {
                _context.Muestras.Remove(muestra);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarProyectosAsync()
        {
            ViewData["Proyectos"] = new SelectList(
                await _context.Proyectos.AsNoTracking().OrderBy(p => p.Titulo).ToListAsync(),
                "IdProyecto",
                "Titulo");
        }

        private async Task<bool> ExisteAsync(int id)
        {
            return await _context.Muestras.AnyAsync(e => e.IdMuestra == id);
        }
    }
}
