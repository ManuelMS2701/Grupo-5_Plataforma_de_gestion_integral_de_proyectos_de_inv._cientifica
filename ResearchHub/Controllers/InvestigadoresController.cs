using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class InvestigadoresController : Controller
    {
        private readonly ResearchHubContext _context;

        public InvestigadoresController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.Investigadores
                .Include(i => i.Institucion)
                .AsNoTracking();
            var totalItems = await query.CountAsync();
            var investigadores = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(investigadores);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var investigador = await _context.Investigadores
                .Include(i => i.Institucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdInvestigador == id.Value);

            if (investigador == null) return NotFound();

            return View(investigador);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Instituciones"] = await _context.Instituciones
                .AsNoTracking()
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Investigador investigador)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Instituciones"] = await _context.Instituciones
                    .AsNoTracking()
                    .ToListAsync();
                return View(investigador);
            }

            investigador.FechaRegistro = DateTime.UtcNow;
            _context.Investigadores.Add(investigador);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var investigador = await _context.Investigadores.FindAsync(id.Value);
            if (investigador == null) return NotFound();

            ViewData["Instituciones"] = await _context.Instituciones
                .AsNoTracking()
                .ToListAsync();

            return View(investigador);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Investigador investigador)
        {
            if (id != investigador.IdInvestigador) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Instituciones"] = await _context.Instituciones
                    .AsNoTracking()
                    .ToListAsync();
                return View(investigador);
            }

            try
            {
                _context.Update(investigador);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await InvestigadorExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var investigador = await _context.Investigadores
                .Include(i => i.Institucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdInvestigador == id.Value);

            if (investigador == null) return NotFound();

            return View(investigador);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var investigador = await _context.Investigadores.FindAsync(id);
            if (investigador != null)
            {
                _context.Investigadores.Remove(investigador);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> InvestigadorExists(int id)
        {
            return await _context.Investigadores.AnyAsync(e => e.IdInvestigador == id);
        }
    }
}
