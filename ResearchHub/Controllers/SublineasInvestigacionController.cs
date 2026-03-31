using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class SublineasInvestigacionController : Controller
    {
        private readonly ResearchHubContext _context;

        public SublineasInvestigacionController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.SublineasInvestigacion
                .Include(s => s.LineaInvestigacion)
                .AsNoTracking()
                .OrderBy(s => s.LineaInvestigacion!.Nombre)
                .ThenBy(s => s.Nombre)
                .ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.SublineasInvestigacion
                .Include(s => s.LineaInvestigacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdSublinea == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarLineasAsync();
            return View(new SublineaInvestigacion { Activa = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SublineaInvestigacion model)
        {
            if (!ModelState.IsValid)
            {
                await CargarLineasAsync();
                return View(model);
            }

            _context.SublineasInvestigacion.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.SublineasInvestigacion.FindAsync(id.Value);
            if (model == null) return NotFound();

            await CargarLineasAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SublineaInvestigacion model)
        {
            if (id != model.IdSublinea) return NotFound();

            if (!ModelState.IsValid)
            {
                await CargarLineasAsync();
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.SublineasInvestigacion.AnyAsync(s => s.IdSublinea == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.SublineasInvestigacion
                .Include(s => s.LineaInvestigacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdSublinea == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.SublineasInvestigacion.FindAsync(id);
            if (model != null)
            {
                _context.SublineasInvestigacion.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarLineasAsync()
        {
            ViewData["IdLinea"] = new SelectList(
                await _context.LineasInvestigacion.AsNoTracking().OrderBy(l => l.Nombre).ToListAsync(),
                "IdLinea",
                "Nombre");
        }
    }
}
