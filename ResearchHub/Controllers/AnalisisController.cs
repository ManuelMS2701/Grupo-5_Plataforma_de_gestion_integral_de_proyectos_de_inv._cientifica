using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class AnalisisController : Controller
    {
        private readonly ResearchHubContext _context;

        public AnalisisController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.Analisis
                .Include(a => a.Resultado)
                    .ThenInclude(r => r!.Experimento)
                .AsNoTracking()
                .OrderByDescending(xd => xd.Fecha);
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
            var model = await _context.Analisis
                .Include(a => a.Resultado)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdAnalisis == id);
            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Analisis { Fecha = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Analisis model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await CargarCombosAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Analisis.FindAsync(id);
            if (model == null) return NotFound();
            await CargarCombosAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Analisis model)
        {
            if (id != model.IdAnalisis) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Analisis.AnyAsync(e => e.IdAnalisis == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await CargarCombosAsync();
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Analisis
                .Include(a => a.Resultado)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdAnalisis == id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Analisis.FindAsync(id);
            if (model != null)
            {
                _context.Analisis.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombosAsync()
        {
            var resultados = await _context.Resultados
                .Include(r => r.Experimento)
                .AsNoTracking()
                .ToListAsync();
                
            var items = resultados.Select(r => new { 
                Id = r.IdResultado, 
                Name = $"Res #{r.IdResultado} - Exp: {r.Experimento?.Titulo}" 
            }).ToList();
            
            ViewData["IdResultado"] = new SelectList(items, "Id", "Name");
        }
    }
}

