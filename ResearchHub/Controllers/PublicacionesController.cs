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
    public class PublicacionesController : Controller
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;
        private readonly ResearchHubContext _context;

        public PublicacionesController(ResearchHubContext context)
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

            return View(new PagedListViewModel<Publicacion>
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
            sb.AppendLine("Id,Titulo,Revista,FechaPublicacion,DOI,Proyecto");
            foreach (var item in data)
            {
                sb.AppendLine(string.Join(',',
                    item.IdPublicacion,
                    Escape(item.Titulo),
                    Escape(item.Revista),
                    Escape(item.FechaPublicacion?.ToString("yyyy-MM-dd")),
                    Escape(item.DOI),
                    Escape(item.Proyecto?.Titulo)));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "publicaciones.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Publicaciones
                .Include(p => p.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdPublicacion == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Publicacion { FechaPublicacion = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publicacion model)
        {
            if (ModelState.IsValid)
            {
                _context.Publicaciones.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarCombosAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Publicaciones.FindAsync(id.Value);
            if (model == null) return NotFound();

            await CargarCombosAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publicacion model)
        {
            if (id != model.IdPublicacion) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Publicaciones.AnyAsync(e => e.IdPublicacion == id))
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

            var model = await _context.Publicaciones
                .Include(p => p.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdPublicacion == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Publicaciones.FindAsync(id);
            if (model != null)
            {
                _context.Publicaciones.Remove(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Publicacion> BuildQuery(string? q, string? sort)
        {
            var query = _context.Publicaciones
                .Include(p => p.Proyecto)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p =>
                    p.Titulo.Contains(term) ||
                    (p.Revista != null && p.Revista.Contains(term)) ||
                    (p.DOI != null && p.DOI.Contains(term)) ||
                    (p.Proyecto != null && p.Proyecto.Titulo.Contains(term)));
            }

            return sort switch
            {
                "titulo_asc" => query.OrderBy(p => p.Titulo),
                "titulo_desc" => query.OrderByDescending(p => p.Titulo),
                "fecha_asc" => query.OrderBy(p => p.FechaPublicacion).ThenBy(p => p.Titulo),
                _ => query.OrderByDescending(p => p.FechaPublicacion).ThenBy(p => p.Titulo)
            };
        }

        private async Task CargarCombosAsync()
        {
            ViewData["IdProyecto"] = new SelectList(
                await _context.Proyectos.AsNoTracking().OrderBy(p => p.Titulo).ToListAsync(),
                "IdProyecto",
                "Titulo");
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
