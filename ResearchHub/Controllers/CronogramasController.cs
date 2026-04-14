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
    public class CronogramasController : Controller
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;
        private readonly ResearchHubContext _context;

        public CronogramasController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q, string? sort = "inicio_desc", int page = 1, int pageSize = DefaultPageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, MaxPageSize);

            var query = BuildQuery(q, sort);
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(new PagedListViewModel<Cronograma>
            {
                Items = items,
                Query = q,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        public async Task<IActionResult> ExportCsv(string? q, string? sort = "inicio_desc")
        {
            var data = await BuildQuery(q, sort).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Fase,Estado,Avance,Hito,Responsable,Prioridad,Riesgo,Inicio,Fin,Dependencia,Proyecto");
            foreach (var item in data)
            {
                sb.AppendLine(string.Join(',',
                    item.IdCronograma,
                    Escape(item.NombreFase),
                    Escape(item.Estado),
                    item.PorcentajeAvance,
                    item.EsHito ? "Si" : "No",
                    Escape(item.Responsable),
                    Escape(item.Prioridad),
                    Escape(item.Riesgo),
                    Escape(item.FechaInicio?.ToString("yyyy-MM-dd")),
                    Escape(item.FechaFin?.ToString("yyyy-MM-dd")),
                    Escape(item.Dependencia?.NombreFase),
                    Escape(item.Proyecto?.Titulo)));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "cronogramas.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Cronogramas
                .Include(c => c.Proyecto)
                .Include(c => c.Dependencia)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdCronograma == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Cronograma
            {
                FechaInicio = DateTime.UtcNow.Date,
                PorcentajeAvance = 0,
                Prioridad = "Media",
                Riesgo = "Bajo"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cronograma model)
        {
            ValidarDependencia(model);

            if (ModelState.IsValid)
            {
                _context.Cronogramas.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarCombosAsync(model.IdProyecto, model.IdDependencia);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Cronogramas.FindAsync(id.Value);
            if (model == null) return NotFound();

            await CargarCombosAsync(model.IdProyecto, model.IdDependencia, model.IdCronograma);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cronograma model)
        {
            if (id != model.IdCronograma) return NotFound();

            ValidarDependencia(model);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Cronogramas.AnyAsync(e => e.IdCronograma == id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarCombosAsync(model.IdProyecto, model.IdDependencia, model.IdCronograma);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.Cronogramas
                .Include(c => c.Proyecto)
                .Include(c => c.Dependencia)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdCronograma == id.Value);

            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Cronogramas.FindAsync(id);
            if (model != null)
            {
                _context.Cronogramas.Remove(model);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Cronograma> BuildQuery(string? q, string? sort)
        {
            var query = _context.Cronogramas
                .Include(c => c.Proyecto)
                .Include(c => c.Dependencia)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    c.NombreFase.Contains(term) ||
                    (c.Estado != null && c.Estado.Contains(term)) ||
                    (c.Responsable != null && c.Responsable.Contains(term)) ||
                    (c.Prioridad != null && c.Prioridad.Contains(term)) ||
                    (c.Riesgo != null && c.Riesgo.Contains(term)) ||
                    (c.Proyecto != null && c.Proyecto.Titulo.Contains(term)));
            }

            return sort switch
            {
                "fase_asc" => query.OrderBy(c => c.NombreFase),
                "fase_desc" => query.OrderByDescending(c => c.NombreFase),
                "avance_desc" => query.OrderByDescending(c => c.PorcentajeAvance),
                "hito_desc" => query.OrderByDescending(c => c.EsHito).ThenBy(c => c.FechaFin),
                "inicio_asc" => query.OrderBy(c => c.FechaInicio).ThenBy(c => c.NombreFase),
                _ => query.OrderByDescending(c => c.FechaInicio).ThenBy(c => c.NombreFase)
            };
        }

        private async Task CargarCombosAsync(int? idProyecto = null, int? idDependencia = null, int? idActual = null)
        {
            ViewData["IdProyecto"] = new SelectList(
                await _context.Proyectos.AsNoTracking().OrderBy(p => p.Titulo).ToListAsync(),
                "IdProyecto",
                "Titulo",
                idProyecto);

            var dependencias = _context.Cronogramas
                .AsNoTracking()
                .Include(c => c.Proyecto)
                .AsQueryable();

            if (idProyecto.HasValue)
            {
                dependencias = dependencias.Where(c => c.IdProyecto == idProyecto.Value);
            }

            if (idActual.HasValue)
            {
                dependencias = dependencias.Where(c => c.IdCronograma != idActual.Value);
            }

            ViewData["IdDependencia"] = new SelectList(
                await dependencias
                    .OrderBy(c => c.NombreFase)
                    .Select(c => new { c.IdCronograma, Nombre = (c.Proyecto != null ? c.Proyecto.Titulo + " / " : string.Empty) + c.NombreFase })
                    .ToListAsync(),
                "IdCronograma",
                "Nombre",
                idDependencia);

            ViewData["EstadosCronograma"] = new SelectList(
                new[]
                {
                    new { Value = "Pendiente", Text = "Pendiente" },
                    new { Value = "En progreso", Text = "En progreso" },
                    new { Value = "Completado", Text = "Completado" },
                    new { Value = "Bloqueado", Text = "Bloqueado" },
                    new { Value = "Cancelado", Text = "Cancelado" }
                },
                "Value",
                "Text");
        }

        private void ValidarDependencia(Cronograma model)
        {
            if (!model.IdDependencia.HasValue)
            {
                return;
            }

            if (model.IdDependencia == model.IdCronograma)
            {
                ModelState.AddModelError(nameof(Cronograma.IdDependencia), "Una fase no puede depender de sí misma.");
                return;
            }

            var existe = _context.Cronogramas.Any(c => c.IdCronograma == model.IdDependencia && c.IdProyecto == model.IdProyecto);
            if (!existe)
            {
                ModelState.AddModelError(nameof(Cronograma.IdDependencia), "La dependencia debe pertenecer al mismo proyecto.");
            }
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
