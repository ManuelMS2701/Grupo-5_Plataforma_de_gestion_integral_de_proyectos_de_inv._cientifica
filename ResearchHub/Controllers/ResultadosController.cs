using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class ResultadosController : Controller
    {
        private readonly ResearchHubContext _context;

        public ResultadosController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var resultados = await _context.Resultados
                .Include(r => r.Experimento)
                .Include(r => r.Variable)
                .AsNoTracking()
                .OrderByDescending(r => r.FechaRegistro)
                .ToListAsync();

            return View(resultados);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var resultado = await _context.Resultados
                .Include(r => r.Experimento)
                .Include(r => r.Variable)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdResultado == id.Value);

            if (resultado == null) return NotFound();

            return View(resultado);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Resultado { FechaRegistro = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resultado resultado)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync();
                return View(resultado);
            }

            _context.Resultados.Add(resultado);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resultado = await _context.Resultados.FindAsync(id.Value);
            if (resultado == null) return NotFound();

            await CargarCombosAsync();
            return View(resultado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resultado resultado)
        {
            if (id != resultado.IdResultado) return NotFound();

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync();
                return View(resultado);
            }

            try
            {
                _context.Update(resultado);
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

            var resultado = await _context.Resultados
                .Include(r => r.Experimento)
                .Include(r => r.Variable)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdResultado == id.Value);

            if (resultado == null) return NotFound();

            return View(resultado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultado = await _context.Resultados.FindAsync(id);
            if (resultado != null)
            {
                _context.Resultados.Remove(resultado);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombosAsync()
        {
            ViewData["Experimentos"] = new SelectList(
                await _context.Experimentos.AsNoTracking().OrderBy(e => e.Titulo).ToListAsync(),
                "IdExperimento",
                "Titulo");

            ViewData["Variables"] = new SelectList(
                await _context.Variables.AsNoTracking().OrderBy(v => v.Nombre).ToListAsync(),
                "IdVariable",
                "Nombre");
        }

        private async Task<bool> ExisteAsync(int id)
        {
            return await _context.Resultados.AnyAsync(e => e.IdResultado == id);
        }
    }
}
