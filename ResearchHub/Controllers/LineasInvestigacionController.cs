using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    public class LineasInvestigacionController : Controller
    {
        private readonly ResearchHubContext _context;

        public LineasInvestigacionController(ResearchHubContext context)
        {
            _context = context;
        }

        // GET: LineasInvestigacion
        public async Task<IActionResult> Index()
        {
            var lineas = await _context.LineasInvestigacion
                .AsNoTracking()
                .ToListAsync();

            return View(lineas);
        }

        // GET: LineasInvestigacion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var linea = await _context.LineasInvestigacion
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdLinea == id.Value);

            if (linea == null) return NotFound();

            return View(linea);
        }

        // GET: LineasInvestigacion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LineasInvestigacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LineaInvestigacion linea)
        {
            if (!ModelState.IsValid)
            {
                return View(linea);
            }

            _context.LineasInvestigacion.Add(linea);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: LineasInvestigacion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var linea = await _context.LineasInvestigacion.FindAsync(id.Value);
            if (linea == null) return NotFound();

            return View(linea);
        }

        // POST: LineasInvestigacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LineaInvestigacion linea)
        {
            if (id != linea.IdLinea) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(linea);
            }

            try
            {
                _context.Update(linea);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LineaExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: LineasInvestigacion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var linea = await _context.LineasInvestigacion
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdLinea == id.Value);

            if (linea == null) return NotFound();

            return View(linea);
        }

        // POST: LineasInvestigacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linea = await _context.LineasInvestigacion.FindAsync(id);
            if (linea != null)
            {
                _context.LineasInvestigacion.Remove(linea);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> LineaExists(int id)
        {
            return await _context.LineasInvestigacion.AnyAsync(e => e.IdLinea == id);
        }
    }
}
