using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class LaboratoriosController : Controller
    {
        private readonly ResearchHubContext _context;

        public LaboratoriosController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Laboratorios.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Laboratorios.AsNoTracking().FirstOrDefaultAsync(m => m.IdLaboratorio == id);
            if (model == null) return NotFound();
            return View(model);
        }

        public IActionResult Create()
        {
            return View(new Laboratorio { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Laboratorio model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Laboratorios.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Laboratorio model)
        {
            if (id != model.IdLaboratorio) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Laboratorios.AnyAsync(e => e.IdLaboratorio == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Laboratorios.AsNoTracking().FirstOrDefaultAsync(m => m.IdLaboratorio == id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Laboratorios.FindAsync(id);
            if (model != null)
            {
                _context.Laboratorios.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
