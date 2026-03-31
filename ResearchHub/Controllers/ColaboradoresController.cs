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
    public class ColaboradoresController : Controller
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;
        private readonly ResearchHubContext _context;

        public ColaboradoresController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q, string? sort = "nombre_asc", int page = 1, int pageSize = DefaultPageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, MaxPageSize);

            var query = BuildQuery(q, sort);
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(new PagedListViewModel<Colaborador>
            {
                Items = items,
                Query = q,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        public async Task<IActionResult> ExportCsv(string? q, string? sort = "nombre_asc")
        {
            var data = await BuildQuery(q, sort).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Nombre,Apellido,Email,Tipo,Rol,Proyecto");
            foreach (var item in data)
            {
                sb.AppendLine(string.Join(',',
                    item.IdColaborador,
                    Escape(item.Nombre),
                    Escape(item.Apellido),
                    Escape(item.Email),
                    Escape(item.Tipo),
                    Escape(item.Rol),
                    Escape(item.Proyecto?.Titulo)));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "colaboradores.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Colaboradores
                .Include(c => c.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdColaborador == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Colaborador());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Colaborador model)
        {
            if (ModelState.IsValid)
            {
                _context.Colaboradores.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarCombosAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Colaboradores.FindAsync(id.Value);
            if (model == null) return NotFound();

            await CargarCombosAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Colaborador model)
        {
            if (id != model.IdColaborador) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Colaboradores.AnyAsync(e => e.IdColaborador == id))
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

            var model = await _context.Colaboradores
                .Include(c => c.Proyecto)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdColaborador == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Colaboradores.FindAsync(id);
            if (model != null)
            {
                _context.Colaboradores.Remove(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Colaborador> BuildQuery(string? q, string? sort)
        {
            var query = _context.Colaboradores
                .Include(c => c.Proyecto)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    c.Nombre.Contains(term) ||
                    c.Apellido.Contains(term) ||
                    (c.Email != null && c.Email.Contains(term)) ||
                    (c.Tipo != null && c.Tipo.Contains(term)) ||
                    (c.Rol != null && c.Rol.Contains(term)) ||
                    (c.Proyecto != null && c.Proyecto.Titulo.Contains(term)));
            }

            return sort switch
            {
                "nombre_desc" => query.OrderByDescending(c => c.Apellido).ThenByDescending(c => c.Nombre),
                "rol_asc" => query.OrderBy(c => c.Rol).ThenBy(c => c.Apellido),
                "rol_desc" => query.OrderByDescending(c => c.Rol).ThenBy(c => c.Apellido),
                _ => query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre)
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
