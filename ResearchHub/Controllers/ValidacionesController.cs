using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class ValidacionesController : Controller
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;
        private readonly ResearchHubContext _context;

        public ValidacionesController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q, string? sort = "fecha_desc", int page = 1, int pageSize = DefaultPageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, MaxPageSize);

            var query = BuildQuery(q, sort);
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(new PagedListViewModel<Validacion>
            {
                Items = items,
                Query = q,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        public async Task<IActionResult> ExportCsv(string? q, string? sort = "fecha_desc")
        {
            var data = await BuildQuery(q, sort).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Fecha,Resultado,Validador,Analisis,Experimento");
            foreach (var item in data)
            {
                sb.AppendLine(string.Join(',',
                    item.IdValidacion,
                    Escape(item.Fecha?.ToString("yyyy-MM-dd")),
                    Escape(item.Resultado),
                    Escape(item.Validador),
                    item.IdAnalisis,
                    Escape(item.Analisis?.Resultado?.Experimento?.Titulo)));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "validaciones.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Validaciones
                .Include(v => v.Analisis)
                    .ThenInclude(a => a!.Resultado)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdValidacion == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Validacion { Fecha = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Validacion model)
        {
            if (ModelState.IsValid)
            {
                _context.Validaciones.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarCombosAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Validaciones.FindAsync(id.Value);
            if (model == null) return NotFound();

            await CargarCombosAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Validacion model)
        {
            if (id != model.IdValidacion) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Validaciones.AnyAsync(e => e.IdValidacion == id))
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

            var model = await _context.Validaciones
                .Include(v => v.Analisis)
                    .ThenInclude(a => a!.Resultado)
                        .ThenInclude(r => r!.Experimento)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdValidacion == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Validaciones.FindAsync(id);
            if (model != null)
            {
                _context.Validaciones.Remove(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Validacion> BuildQuery(string? q, string? sort)
        {
            var query = _context.Validaciones
                .Include(v => v.Analisis)
                    .ThenInclude(a => a!.Resultado)
                        .ThenInclude(r => r!.Experimento)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(v =>
                    (v.Resultado != null && v.Resultado.Contains(term)) ||
                    (v.Validador != null && v.Validador.Contains(term)) ||
                    (v.Observaciones != null && v.Observaciones.Contains(term)) ||
                    (v.Analisis != null && v.Analisis.Resultado != null && v.Analisis.Resultado.Experimento != null && v.Analisis.Resultado.Experimento.Titulo.Contains(term)));
            }

            return sort switch
            {
                "resultado_asc" => query.OrderBy(v => v.Resultado).ThenByDescending(v => v.Fecha),
                "resultado_desc" => query.OrderByDescending(v => v.Resultado).ThenByDescending(v => v.Fecha),
                "fecha_asc" => query.OrderBy(v => v.Fecha),
                _ => query.OrderByDescending(v => v.Fecha)
            };
        }

        private async Task CargarCombosAsync()
        {
            var analisis = await _context.Analisis
                .Include(a => a.Resultado)
                    .ThenInclude(r => r!.Experimento)
                .AsNoTracking()
                .ToListAsync();

            var items = analisis
                .Select(a => new
                {
                    Id = a.IdAnalisis,
                    Name = $"An #{a.IdAnalisis} - Exp: {a.Resultado?.Experimento?.Titulo}"
                })
                .ToList();

            ViewData["IdAnalisis"] = new SelectList(items, "Id", "Name");
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
                return value;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
