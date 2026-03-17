using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    public class InstitucionesController : Controller
    {
        private readonly ResearchHubContext _context;

        public InstitucionesController(ResearchHubContext context)
        {
            _context = context;
        }

        // GET: Instituciones
        public async Task<IActionResult> Index()
        {
            var instituciones = await _context.Instituciones
                .AsNoTracking()
                .ToListAsync();

            return View(instituciones);
        }

        // GET: Instituciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var institucion = await _context.Instituciones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdInstitucion == id.Value);

            if (institucion == null) return NotFound();

            return View(institucion);
        }

        // GET: Instituciones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Instituciones/Create
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

        // GET: Instituciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var institucion = await _context.Instituciones.FindAsync(id.Value);
            if (institucion == null) return NotFound();

            return View(institucion);
        }

        // POST: Instituciones/Edit/5
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

        // GET: Instituciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var institucion = await _context.Instituciones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdInstitucion == id.Value);

            if (institucion == null) return NotFound();

            return View(institucion);
        }

        // POST: Instituciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var institucion = await _context.Instituciones.FindAsync(id);
            if (institucion != null)
            {
                _context.Instituciones.Remove(institucion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> InstitucionExists(int id)
        {
            return await _context.Instituciones.AnyAsync(e => e.IdInstitucion == id);
        }
    }
}
