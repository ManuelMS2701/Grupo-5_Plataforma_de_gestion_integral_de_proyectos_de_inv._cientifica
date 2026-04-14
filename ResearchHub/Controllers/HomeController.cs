using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ResearchHubContext _context;

        public HomeController(ResearchHubContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Dashboard()
        {
            var projectIds = await ObtenerProjectIdsAsync();

            var proyectosQuery = _context.Proyectos
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto));

            var totalProyectos = await proyectosQuery.CountAsync();
            var proyectosActivos = await proyectosQuery.CountAsync(p => p.Estado != null && p.Estado.ToLower() == "activo");

            var totalExperimentos = await _context.Experimentos
                .AsNoTracking()
                .CountAsync(e => projectIds.Contains(e.IdProyecto));

            var experimentoIds = await _context.Experimentos
                .AsNoTracking()
                .Where(e => projectIds.Contains(e.IdProyecto))
                .Select(e => e.IdExperimento)
                .ToListAsync();

            var totalResultados = await _context.Resultados
                .AsNoTracking()
                .CountAsync(r => experimentoIds.Contains(r.IdExperimento));

            var totalTareasPendientes = await _context.TareasInvestigacion
                .AsNoTracking()
                .CountAsync(t => projectIds.Contains(t.IdProyecto) && (t.Estado == null || !t.Estado.ToLower().Contains("complet")));

            var totalHitosPendientes = await _context.Cronogramas
                .AsNoTracking()
                .CountAsync(c => projectIds.Contains(c.IdProyecto) && c.EsHito && (c.Estado == null || !c.Estado.ToLower().Contains("complet")));

            var proyectosRecientes = await proyectosQuery
                .OrderByDescending(p => p.FechaCreacion)
                .Take(6)
                .Select(p => new DashboardProyectoItem
                {
                    IdProyecto = p.IdProyecto,
                    Titulo = p.Titulo,
                    Institucion = p.Institucion != null ? p.Institucion.Nombre : null,
                    Estado = p.Estado,
                    FechaInicio = p.FechaInicio
                })
                .ToListAsync();

            var tareasPendientes = await _context.TareasInvestigacion
                .AsNoTracking()
                .Where(t => projectIds.Contains(t.IdProyecto) && (t.Estado == null || !t.Estado.ToLower().Contains("complet")))
                .OrderBy(t => t.FechaLimite ?? DateTime.MaxValue)
                .Take(6)
                .Select(t => new DashboardTareaItem
                {
                    IdTarea = t.IdTarea,
                    Titulo = t.Titulo,
                    Proyecto = t.Proyecto != null ? t.Proyecto.Titulo : null,
                    Estado = t.Estado,
                    FechaLimite = t.FechaLimite
                })
                .ToListAsync();

            var hitosProximos = await _context.Cronogramas
                .AsNoTracking()
                .Where(c => projectIds.Contains(c.IdProyecto) && c.EsHito && (c.Estado == null || !c.Estado.ToLower().Contains("complet")))
                .OrderBy(c => c.FechaFin ?? c.FechaInicio)
                .Take(6)
                .Select(c => new DashboardHitoItem
                {
                    IdCronograma = c.IdCronograma,
                    NombreFase = c.NombreFase,
                    Proyecto = c.Proyecto != null ? c.Proyecto.Titulo : null,
                    FechaObjetivo = c.FechaFin ?? c.FechaInicio,
                    Estado = c.Estado
                })
                .ToListAsync();

            var actividadResultados = await _context.Resultados
                .AsNoTracking()
                .Where(r => experimentoIds.Contains(r.IdExperimento))
                .OrderByDescending(r => r.FechaRegistro)
                .Take(4)
                .Select(r => new DashboardActividadItem
                {
                    Tipo = "Resultado",
                    Titulo = "Resultado #" + r.IdResultado,
                    Proyecto = r.Experimento != null && r.Experimento.Proyecto != null ? r.Experimento.Proyecto.Titulo : null,
                    Fecha = r.FechaRegistro,
                    Controller = "Resultados",
                    Id = r.IdResultado
                })
                .ToListAsync();

            var actividadPublicaciones = await _context.Publicaciones
                .AsNoTracking()
                .Where(p => projectIds.Contains(p.IdProyecto) && p.FechaPublicacion.HasValue)
                .OrderByDescending(p => p.FechaPublicacion)
                .Take(4)
                .Select(p => new DashboardActividadItem
                {
                    Tipo = "Publicacion",
                    Titulo = p.Titulo,
                    Proyecto = p.Proyecto != null ? p.Proyecto.Titulo : null,
                    Fecha = p.FechaPublicacion ?? DateTime.MinValue,
                    Controller = "Publicaciones",
                    Id = p.IdPublicacion
                })
                .ToListAsync();

            var actividadBitacora = await _context.BitacoraProyecto
                .AsNoTracking()
                .Where(b => projectIds.Contains(b.IdProyecto))
                .OrderByDescending(b => b.FechaRegistro)
                .Take(4)
                .Select(b => new DashboardActividadItem
                {
                    Tipo = "Bitacora",
                    Titulo = b.Titulo,
                    Proyecto = b.Proyecto != null ? b.Proyecto.Titulo : null,
                    Fecha = b.FechaRegistro,
                    Controller = "MiEspacio",
                    Id = b.IdBitacora
                })
                .ToListAsync();

            var actividad = actividadResultados
                .Concat(actividadPublicaciones)
                .Concat(actividadBitacora)
                .OrderByDescending(a => a.Fecha)
                .Take(10)
                .ToList();

            var vm = new HomeDashboardViewModel
            {
                NombreUsuario = User.FindFirstValue("display_name") ?? User.Identity?.Name ?? "Usuario",
                Rol = User.FindFirstValue(ClaimTypes.Role) ?? Roles.Investigador,
                TotalProyectos = totalProyectos,
                ProyectosActivos = proyectosActivos,
                TotalExperimentos = totalExperimentos,
                TotalResultados = totalResultados,
                TotalTareasPendientes = totalTareasPendientes,
                TotalHitosPendientes = totalHitosPendientes,
                ProyectosRecientes = proyectosRecientes,
                TareasPendientes = tareasPendientes,
                HitosProximos = hitosProximos,
                ActividadReciente = actividad
            };

            return View("Index", vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<List<int>> ObtenerProjectIdsAsync()
        {
            if (User.IsInRole(Roles.Administrador))
            {
                return await _context.Proyectos.AsNoTracking().Select(p => p.IdProyecto).ToListAsync();
            }

            var email = User.Identity?.Name ?? string.Empty;
            var investigadorIdClaim = User.FindFirstValue("investigador_id");
            var tieneInvestigadorId = int.TryParse(investigadorIdClaim, out var investigadorId);

            return await _context.Proyectos
                .AsNoTracking()
                .Where(p =>
                    (tieneInvestigadorId && p.IdInvestigadorPrincipal == investigadorId) ||
                    (p.InvestigadorPrincipal != null && p.InvestigadorPrincipal.Email == email) ||
                    p.Colaboradores.Any(c => c.Email == email))
                .Select(p => p.IdProyecto)
                .Distinct()
                .ToListAsync();
        }
    }
}
