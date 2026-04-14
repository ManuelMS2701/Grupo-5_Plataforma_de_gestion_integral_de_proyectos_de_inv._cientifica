using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class ProtocolosController : Controller
    {
        private readonly ResearchHubContext _context;

        public ProtocolosController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var query = _context.Protocolos
                .AsNoTracking()
                .OrderBy(p => p.Nombre);
            var totalItems = await query.CountAsync();
            var protocolos = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            return View(protocolos);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var protocolo = await _context.Protocolos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProtocolo == id.Value);

            if (protocolo == null) return NotFound();

            ViewData["ExperimentosAsociados"] = await _context.Experimentos
                .AsNoTracking()
                .CountAsync(e => e.IdProtocolo == protocolo.IdProtocolo);

            return View(protocolo);
        }

        public IActionResult Create()
        {
            return View(new Protocolo { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Protocolo protocolo)
        {
            if (!ModelState.IsValid)
            {
                return View(protocolo);
            }

            _context.Protocolos.Add(protocolo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var protocolo = await _context.Protocolos.FindAsync(id.Value);
            if (protocolo == null) return NotFound();

            return View(protocolo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Protocolo protocolo)
        {
            if (id != protocolo.IdProtocolo) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(protocolo);
            }

            try
            {
                _context.Update(protocolo);
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

            var protocolo = await _context.Protocolos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProtocolo == id.Value);

            if (protocolo == null) return NotFound();

            ViewData["TieneExperimentos"] = await _context.Experimentos
                .AsNoTracking()
                .AnyAsync(e => e.IdProtocolo == protocolo.IdProtocolo);

            return View(protocolo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var protocolo = await _context.Protocolos.FindAsync(id);
            if (protocolo == null) return RedirectToAction(nameof(Index));

            var tieneExperimentos = await _context.Experimentos
                .AsNoTracking()
                .AnyAsync(e => e.IdProtocolo == id);

            if (tieneExperimentos)
            {
                TempData["DeleteError"] = "No se puede eliminar: existen experimentos asociados.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Protocolos.Remove(protocolo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ExisteAsync(int id)
        {
            return await _context.Protocolos.AnyAsync(e => e.IdProtocolo == id);
        }
    }
}
