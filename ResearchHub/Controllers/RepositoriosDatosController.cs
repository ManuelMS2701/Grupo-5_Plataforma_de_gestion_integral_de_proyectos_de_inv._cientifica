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
    public class RepositoriosDatosController : Controller
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;
        private readonly ResearchHubContext _context;

        public RepositoriosDatosController(ResearchHubContext context)
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

            return View(new PagedListViewModel<RepositorioDatos>
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
            sb.AppendLine("Id,Nombre,Tipo,Url,FechaRegistro,Proyecto");
            foreach (var item in data)
            {
                sb.AppendLine(string.Join(',',
                    item.IdRepositorio,
                    Escape(item.Nombre),
                    Escape(item.Tipo),
                    Escape(item.Url),
                    Escape(item.FechaRegistro?.ToString("yyyy-MM-dd")),
                    Escape(item.Proyecto?.Titulo)));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "repositorios_datos.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.RepositoriosDatos
                .Include(r => r.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdRepositorio == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new RepositorioDatos { FechaRegistro = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepositorioDatos model)
        {
            if (ModelState.IsValid)
            {
                _context.RepositoriosDatos.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarCombosAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.RepositoriosDatos.FindAsync(id.Value);
            if (model == null) return NotFound();

            await CargarCombosAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RepositorioDatos model)
        {
            if (id != model.IdRepositorio) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.RepositoriosDatos.AnyAsync(e => e.IdRepositorio == id))
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

            var model = await _context.RepositoriosDatos
                .Include(r => r.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdRepositorio == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.RepositoriosDatos.FindAsync(id);
            if (model != null)
            {
                _context.RepositoriosDatos.Remove(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<RepositorioDatos> BuildQuery(string? q, string? sort)
        {
            var query = _context.RepositoriosDatos
                .Include(r => r.Proyecto)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(r =>
                    r.Nombre.Contains(term) ||
                    (r.Tipo != null && r.Tipo.Contains(term)) ||
                    (r.Url != null && r.Url.Contains(term)) ||
                    (r.Proyecto != null && r.Proyecto.Titulo.Contains(term)));
            }

            return sort switch
            {
                "nombre_asc" => query.OrderBy(r => r.Nombre),
                "nombre_desc" => query.OrderByDescending(r => r.Nombre),
                "fecha_asc" => query.OrderBy(r => r.FechaRegistro).ThenBy(r => r.Nombre),
                _ => query.OrderByDescending(r => r.FechaRegistro).ThenBy(r => r.Nombre)
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
