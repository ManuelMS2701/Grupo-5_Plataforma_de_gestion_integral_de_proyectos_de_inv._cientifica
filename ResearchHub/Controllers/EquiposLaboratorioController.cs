using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class EquiposLaboratorioController : Controller
    {
        private readonly ResearchHubContext _context;

        public EquiposLaboratorioController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.EquiposLaboratorio
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .OrderBy(e => e.Nombre);
            var totalItems = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(data);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.EquiposLaboratorio
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdEquipo == id);
            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new EquipoLaboratorio { FechaAdquisicion = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EquipoLaboratorio model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await CargarCombosAsync(model.Estado);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.EquiposLaboratorio.FindAsync(id);
            if (model == null) return NotFound();
            await CargarCombosAsync(model.Estado);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EquipoLaboratorio model)
        {
            if (id != model.IdEquipo) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.EquiposLaboratorio.AnyAsync(e => e.IdEquipo == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await CargarCombosAsync(model.Estado);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.EquiposLaboratorio
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdEquipo == id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.EquiposLaboratorio.FindAsync(id);
            if (model != null)
            {
                _context.EquiposLaboratorio.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombosAsync(string? estadoSeleccionado = null)
        {
            var laboratorios = await _context.Laboratorios.AsNoTracking().OrderBy(l => l.Nombre).ToListAsync();
            ViewData["IdLaboratorio"] = new SelectList(laboratorios, "IdLaboratorio", "Nombre");
            ViewData["EstadosEquipo"] = new SelectList(
                new[]
                {
                    new { Value = "Operativo", Text = "Operativo" },
                    new { Value = "En mantenimiento", Text = "En mantenimiento" },
                    new { Value = "Fuera de servicio", Text = "Fuera de servicio" },
                    new { Value = "Baja", Text = "Baja" }
                },
                "Value",
                "Text",
                estadoSeleccionado);
        }
    }
}
