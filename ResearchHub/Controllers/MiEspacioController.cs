using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize]
    public class MiEspacioController : Controller
    {
        private readonly ResearchHubContext _context;

        public MiEspacioController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var email = User.Identity?.Name ?? string.Empty;

            var proyectos = await _context.Proyectos
                .Include(p => p.Institucion)
                .Include(p => p.SublineaInvestigacion)
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto))
                .OrderByDescending(p => p.FechaCreacion)
                .Take(6)
                .ToListAsync();

            var experimentos = await _context.Experimentos
                .Include(e => e.Proyecto)
                .AsNoTracking()
                .Where(e => projectIds.Contains(e.IdProyecto))
                .OrderByDescending(e => e.FechaInicio)
                .Take(6)
                .ToListAsync();

            var resultados = await _context.Resultados
                .Include(r => r.Experimento)
                .AsNoTracking()
                .Where(r => r.Experimento != null && projectIds.Contains(r.Experimento.IdProyecto))
                .OrderByDescending(r => r.FechaRegistro)
                .Take(6)
                .ToListAsync();

            var hitos = await _context.Cronogramas
                .Include(c => c.Proyecto)
                .AsNoTracking()
                .Where(c => projectIds.Contains(c.IdProyecto) && c.EsHito && (c.Estado == null || !c.Estado.ToLower().Contains("complet")))
                .OrderBy(c => c.FechaFin ?? c.FechaInicio)
                .Take(6)
                .ToListAsync();

            var publicaciones = await _context.Publicaciones
                .Include(p => p.Proyecto)
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto))
                .OrderByDescending(p => p.FechaPublicacion)
                .Take(6)
                .ToListAsync();

            var tareas = await _context.TareasInvestigacion
                .Include(t => t.Proyecto)
                .Include(t => t.Experimento)
                .AsNoTracking()
                .Where(t => projectIds.Contains(t.IdProyecto) && (t.Estado == null || !t.Estado.ToLower().Contains("complet")))
                .OrderBy(t => t.FechaLimite ?? DateTime.MaxValue)
                .Take(6)
                .ToListAsync();

            var vm = new MiEspacioViewModel
            {
                NombreUsuario = User.FindFirstValue("display_name") ?? email,
                Rol = User.FindFirstValue(ClaimTypes.Role) ?? Roles.Investigador,
                TotalProyectos = await _context.Proyectos.AsNoTracking().CountAsync(p => projectIds.Contains(p.IdProyecto)),
                TotalExperimentos = await _context.Experimentos.AsNoTracking().CountAsync(e => projectIds.Contains(e.IdProyecto)),
                TotalResultados = await _context.Resultados.AsNoTracking().CountAsync(r => r.Experimento != null && projectIds.Contains(r.Experimento.IdProyecto)),
                TotalPublicaciones = await _context.Publicaciones.AsNoTracking().CountAsync(p => projectIds.Contains(p.IdProyecto)),
                TotalHitosPendientes = await _context.Cronogramas.AsNoTracking().CountAsync(c => projectIds.Contains(c.IdProyecto) && c.EsHito && (c.Estado == null || !c.Estado.ToLower().Contains("complet"))),
                TotalTareasPendientes = await _context.TareasInvestigacion.AsNoTracking().CountAsync(t => projectIds.Contains(t.IdProyecto) && (t.Estado == null || !t.Estado.ToLower().Contains("complet"))),
                ProyectosRecientes = proyectos,
                ExperimentosActivos = experimentos,
                HitosPendientes = hitos,
                ResultadosRecientes = resultados,
                PublicacionesRecientes = publicaciones,
                TareasPendientes = tareas
            };

            return View(vm);
        }

        public async Task<IActionResult> Experimentos()
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var items = await _context.Experimentos
                .Include(e => e.Proyecto)
                .Include(e => e.Laboratorio)
                .AsNoTracking()
                .Where(e => projectIds.Contains(e.IdProyecto))
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Cronograma()
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var items = await _context.Cronogramas
                .Include(c => c.Proyecto)
                .Include(c => c.Dependencia)
                .AsNoTracking()
                .Where(c => projectIds.Contains(c.IdProyecto))
                .OrderBy(c => c.FechaInicio)
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Resultados()
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var items = await _context.Resultados
                .Include(r => r.Experimento)
                    .ThenInclude(e => e!.Proyecto)
                .Include(r => r.Variable)
                .AsNoTracking()
                .Where(r => r.Experimento != null && projectIds.Contains(r.Experimento.IdProyecto))
                .OrderByDescending(r => r.FechaRegistro)
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Publicaciones()
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var items = await _context.Publicaciones
                .Include(p => p.Proyecto)
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto))
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Tareas(int? proyectoId = null)
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var proyectos = await _context.Proyectos
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto))
                .OrderBy(p => p.Titulo)
                .ToListAsync();

            var experimentos = await _context.Experimentos
                .AsNoTracking()
                .Where(e => projectIds.Contains(e.IdProyecto))
                .OrderBy(e => e.Titulo)
                .ToListAsync();

            var query = _context.TareasInvestigacion
                .Include(t => t.Proyecto)
                .Include(t => t.Experimento)
                .Include(t => t.Responsable)
                .AsNoTracking()
                .Where(t => projectIds.Contains(t.IdProyecto));

            if (proyectoId.HasValue)
            {
                query = query.Where(t => t.IdProyecto == proyectoId.Value);
            }

            var tareas = await query
                .OrderBy(t => t.Estado)
                .ThenBy(t => t.FechaLimite)
                .ToListAsync();

            ViewData["PuedeGestionarTareas"] = PuedeGestionarTareas();

            return View(new MisTareasViewModel
            {
                Tareas = tareas,
                Proyectos = proyectos,
                Experimentos = experimentos,
                ProyectoSeleccionado = proyectoId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTarea(int idProyecto, int? idExperimento, string titulo, string? descripcion, string? prioridad, DateTime? fechaLimite)
        {
            if (!PuedeGestionarTareas())
            {
                return Forbid();
            }

            var projectIds = await ObtenerProjectIdsAsync();
            if (!projectIds.Contains(idProyecto))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(titulo))
            {
                TempData["MiEspacioError"] = "El título de la tarea es obligatorio.";
                return RedirectToAction(nameof(Tareas), new { proyectoId = idProyecto });
            }

            var tarea = new TareaInvestigacion
            {
                IdProyecto = idProyecto,
                IdExperimento = idExperimento,
                IdResponsable = ObtenerInvestigadorActualId(),
                Titulo = titulo.Trim(),
                Descripcion = descripcion?.Trim(),
                Prioridad = string.IsNullOrWhiteSpace(prioridad) ? "Media" : prioridad.Trim(),
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow,
                FechaLimite = fechaLimite
            };

            _context.TareasInvestigacion.Add(tarea);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Tareas), new { proyectoId = idProyecto });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoTarea(int idTarea, string estado)
        {
            if (!PuedeGestionarTareas())
            {
                return Forbid();
            }

            var tarea = await _context.TareasInvestigacion.FindAsync(idTarea);
            if (tarea == null) return NotFound();

            var projectIds = await ObtenerProjectIdsAsync();
            if (!projectIds.Contains(tarea.IdProyecto))
            {
                return Forbid();
            }

            tarea.Estado = estado;
            if (!string.IsNullOrWhiteSpace(estado) && estado.ToLower().Contains("complet"))
            {
                tarea.FechaCierre = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Tareas), new { proyectoId = tarea.IdProyecto });
        }

        public async Task<IActionResult> Bitacora(int? proyectoId = null)
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var proyectos = await _context.Proyectos
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto))
                .OrderBy(p => p.Titulo)
                .ToListAsync();

            var query = _context.BitacoraProyecto
                .Include(b => b.Proyecto)
                .Include(b => b.Usuario)
                .AsNoTracking()
                .Where(b => projectIds.Contains(b.IdProyecto));

            if (proyectoId.HasValue)
            {
                query = query.Where(b => b.IdProyecto == proyectoId.Value);
            }

            var entradas = await query
                .OrderByDescending(b => b.FechaRegistro)
                .ToListAsync();

            ViewData["PuedeCrearBitacora"] = PuedeCrearBitacora();

            return View(new BitacoraPageViewModel
            {
                Entradas = entradas,
                Proyectos = proyectos,
                ProyectoSeleccionado = proyectoId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearBitacora(int idProyecto, string titulo, string contenido, string? categoria)
        {
            if (!PuedeCrearBitacora())
            {
                return Forbid();
            }

            var projectIds = await ObtenerProjectIdsAsync();
            if (!projectIds.Contains(idProyecto))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(contenido))
            {
                TempData["MiEspacioError"] = "El título y el contenido de la bitácora son obligatorios.";
                return RedirectToAction(nameof(Bitacora), new { proyectoId = idProyecto });
            }

            var entrada = new BitacoraProyecto
            {
                IdProyecto = idProyecto,
                IdUsuario = ObtenerUsuarioActualId(),
                FechaRegistro = DateTime.UtcNow,
                Titulo = titulo.Trim(),
                Contenido = contenido.Trim(),
                Categoria = categoria?.Trim()
            };

            _context.BitacoraProyecto.Add(entrada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bitacora), new { proyectoId = idProyecto });
        }

        public async Task<IActionResult> Muestras()
        {
            var projectIds = await ObtenerProjectIdsAsync();
            var muestras = await _context.Muestras
                .Include(m => m.Proyecto)
                .AsNoTracking()
                .Where(m => projectIds.Contains(m.IdProyecto))
                .OrderByDescending(m => m.FechaRecoleccion)
                .ToListAsync();

            var seguimientos = await _context.SeguimientosMuestra
                .Include(s => s.Usuario)
                .Include(s => s.Muestra)
                .AsNoTracking()
                .Where(s => s.Muestra != null && projectIds.Contains(s.Muestra.IdProyecto))
                .OrderByDescending(s => s.FechaRegistro)
                .ToListAsync();

            ViewData["PuedeRegistrarSeguimientoMuestra"] = PuedeRegistrarSeguimientoMuestra();

            return View(new MuestrasSeguimientoViewModel
            {
                Muestras = muestras,
                Seguimientos = seguimientos
                    .GroupBy(s => s.IdMuestra)
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<SeguimientoMuestra>)g.ToList())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSeguimientoMuestra(int idMuestra, string estado, string? ubicacion, string? observaciones)
        {
            if (!PuedeRegistrarSeguimientoMuestra())
            {
                return Forbid();
            }

            var muestra = await _context.Muestras.FindAsync(idMuestra);
            if (muestra == null) return NotFound();

            var projectIds = await ObtenerProjectIdsAsync();
            if (!projectIds.Contains(muestra.IdProyecto))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(estado))
            {
                TempData["MiEspacioError"] = "El estado del seguimiento es obligatorio.";
                return RedirectToAction(nameof(Muestras));
            }

            _context.SeguimientosMuestra.Add(new SeguimientoMuestra
            {
                IdMuestra = idMuestra,
                IdUsuario = ObtenerUsuarioActualId(),
                FechaRegistro = DateTime.UtcNow,
                Estado = estado.Trim(),
                Ubicacion = ubicacion?.Trim(),
                Observaciones = observaciones?.Trim()
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Muestras));
        }

        private async Task<List<int>> ObtenerProjectIdsAsync()
        {
            if (User.IsInRole(Roles.Administrador))
            {
                return await _context.Proyectos.AsNoTracking().Select(p => p.IdProyecto).ToListAsync();
            }

            var email = User.Identity?.Name ?? string.Empty;
            var investigadorId = ObtenerInvestigadorActualId();

            return await _context.Proyectos
                .AsNoTracking()
                .Where(p =>
                    (investigadorId.HasValue && p.IdInvestigadorPrincipal == investigadorId.Value) ||
                    (p.InvestigadorPrincipal != null && p.InvestigadorPrincipal.Email == email) ||
                    p.Colaboradores.Any(c => c.Email == email))
                .Select(p => p.IdProyecto)
                .Distinct()
                .ToListAsync();
        }

        private int? ObtenerInvestigadorActualId()
        {
            var claim = User.FindFirstValue("investigador_id");
            return int.TryParse(claim, out var id) ? id : null;
        }

        private int? ObtenerUsuarioActualId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool PuedeGestionarTareas()
        {
            return User.IsInRole(Roles.Administrador) ||
                   User.IsInRole(Roles.Investigador) ||
                   User.IsInRole(Roles.InvestigadorPrincipal) ||
                   User.IsInRole(Roles.TecnicoLaboratorio) ||
                   User.IsInRole(Roles.AnalistaDatos);
        }

        private bool PuedeCrearBitacora()
        {
            return User.IsInRole(Roles.Administrador) ||
                   User.IsInRole(Roles.Investigador) ||
                   User.IsInRole(Roles.InvestigadorPrincipal) ||
                   User.IsInRole(Roles.TecnicoLaboratorio) ||
                   User.IsInRole(Roles.AnalistaDatos) ||
                   User.IsInRole(Roles.RevisorCientifico);
        }

        private bool PuedeRegistrarSeguimientoMuestra()
        {
            return User.IsInRole(Roles.Administrador) ||
                   User.IsInRole(Roles.Investigador) ||
                   User.IsInRole(Roles.InvestigadorPrincipal) ||
                   User.IsInRole(Roles.TecnicoLaboratorio);
        }
    }
}
