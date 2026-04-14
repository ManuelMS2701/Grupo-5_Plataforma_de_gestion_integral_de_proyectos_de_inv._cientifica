using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class ExperimentosController : Controller
    {
        private readonly ResearchHubContext _context;

        public ExperimentosController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.Experimentos
                .Include(e => e.Proyecto)
                .Include(e => e.Protocolo)
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .OrderByDescending(e => e.FechaInicio);
            var totalItems = await query.CountAsync();
            var experimentos = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(experimentos);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var experimento = await _context.Experimentos
                .Include(e => e.Proyecto)
                .Include(e => e.Protocolo)
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdExperimento == id.Value);

            if (experimento == null) return NotFound();

            return View(experimento);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Experimento());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Experimento experimento)
        {
            AgregarErrorSiFechasInvalidas(experimento);

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(experimento.Estado);
                return View(experimento);
            }

            _context.Experimentos.Add(experimento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var experimento = await _context.Experimentos.FindAsync(id.Value);
            if (experimento == null) return NotFound();

            await CargarCombosAsync(experimento.Estado);
            return View(experimento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Experimento experimento)
        {
            if (id != experimento.IdExperimento) return NotFound();

            AgregarErrorSiFechasInvalidas(experimento);

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(experimento.Estado);
                return View(experimento);
            }

            try
            {
                _context.Update(experimento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExisteAsync(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var experimento = await _context.Experimentos
                .Include(e => e.Proyecto)
                .Include(e => e.Protocolo)
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdExperimento == id.Value);

            if (experimento == null) return NotFound();

            return View(experimento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var experimento = await _context.Experimentos.FindAsync(id);
            if (experimento != null)
            {
                _context.Experimentos.Remove(experimento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombosAsync(string? estadoSeleccionado = null)
        {
            ViewData["Proyectos"] = new SelectList(
                await _context.Proyectos.AsNoTracking().OrderBy(p => p.Titulo).ToListAsync(),
                "IdProyecto",
                "Titulo");

            ViewData["Protocolos"] = new SelectList(
                await _context.Protocolos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(),
                "IdProtocolo",
                "Nombre");

            ViewData["Laboratorios"] = new SelectList(
                await _context.Laboratorios.AsNoTracking().OrderBy(l => l.Nombre).ToListAsync(),
                "IdLaboratorio",
                "Nombre");

            ViewData["EstadosExperimento"] = new SelectList(
                new[]
                {
                    new { Value = "Planificado", Text = "Planificado" },
                    new { Value = "En ejecucion", Text = "En ejecucion" },
                    new { Value = "Completado", Text = "Completado" },
                    new { Value = "Cancelado", Text = "Cancelado" }
                },
                "Value",
                "Text",
                estadoSeleccionado);
        }

        private void AgregarErrorSiFechasInvalidas(Experimento experimento)
        {
            if (experimento.FechaInicio.HasValue &&
                experimento.FechaFin.HasValue &&
                experimento.FechaFin.Value.Date < experimento.FechaInicio.Value.Date)
            {
                ModelState.AddModelError(nameof(Experimento.FechaFin), "Fecha fin no puede ser menor a fecha inicio.");
            }
        }

        private async Task<bool> ExisteAsync(int id)
        {
            return await _context.Experimentos.AnyAsync(e => e.IdExperimento == id);
        }
    }
}
