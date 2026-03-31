using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize]
    public class BusquedaController : Controller
    {
        private readonly ResearchHubContext _context;

        public BusquedaController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var hits = new List<SearchHitViewModel>();
            var isAdmin = User?.IsInRole(Roles.Administrador) == true;
            var userEmail = User?.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                const int top = 5;

                var proyectosQuery = _context.Proyectos.AsNoTracking().AsQueryable();
                if (!isAdmin)
                {
                    proyectosQuery = proyectosQuery.Where(p =>
                        (p.InvestigadorPrincipal != null && p.InvestigadorPrincipal.Email == userEmail) ||
                        p.Colaboradores.Any(c => c.Email == userEmail));
                }

                var proyectos = await proyectosQuery
                    .Where(x => x.Titulo.Contains(term) || (x.Descripcion != null && x.Descripcion.Contains(term)))
                    .OrderBy(x => x.Titulo)
                    .Take(top)
                    .Select(x => new SearchHitViewModel
                    {
                        Modulo = "Proyecto",
                        Titulo = x.Titulo,
                        Subtitulo = x.Estado,
                        Controller = "Proyectos",
                        Action = "Details",
                        Id = x.IdProyecto
                    })
                    .ToListAsync();

                hits.AddRange(proyectos);

                if (isAdmin)
                {
                    var experimentos = await _context.Experimentos
                        .AsNoTracking()
                        .Where(x => x.Titulo.Contains(term) || (x.Estado != null && x.Estado.Contains(term)))
                        .OrderByDescending(x => x.FechaInicio)
                        .Take(top)
                        .Select(x => new SearchHitViewModel
                        {
                            Modulo = "Experimento",
                            Titulo = x.Titulo,
                            Subtitulo = x.Estado,
                            Controller = "Experimentos",
                            Action = "Details",
                            Id = x.IdExperimento
                        })
                        .ToListAsync();

                    var publicaciones = await _context.Publicaciones
                        .AsNoTracking()
                        .Where(x => x.Titulo.Contains(term) || (x.Revista != null && x.Revista.Contains(term)) || (x.DOI != null && x.DOI.Contains(term)))
                        .OrderByDescending(x => x.FechaPublicacion)
                        .Take(top)
                        .Select(x => new SearchHitViewModel
                        {
                            Modulo = "Publicacion",
                            Titulo = x.Titulo,
                            Subtitulo = x.Revista,
                            Controller = "Publicaciones",
                            Action = "Details",
                            Id = x.IdPublicacion
                        })
                        .ToListAsync();

                    var repos = await _context.RepositoriosDatos
                        .AsNoTracking()
                        .Where(x => x.Nombre.Contains(term) || (x.Tipo != null && x.Tipo.Contains(term)) || (x.Url != null && x.Url.Contains(term)))
                        .OrderByDescending(x => x.FechaRegistro)
                        .Take(top)
                        .Select(x => new SearchHitViewModel
                        {
                            Modulo = "RepositorioDatos",
                            Titulo = x.Nombre,
                            Subtitulo = x.Tipo,
                            Controller = "RepositoriosDatos",
                            Action = "Details",
                            Id = x.IdRepositorio
                        })
                        .ToListAsync();

                    var colaboradores = await _context.Colaboradores
                        .AsNoTracking()
                        .Where(x => x.Nombre.Contains(term) || x.Apellido.Contains(term) || (x.Email != null && x.Email.Contains(term)))
                        .OrderBy(x => x.Apellido)
                        .ThenBy(x => x.Nombre)
                        .Take(top)
                        .Select(x => new SearchHitViewModel
                        {
                            Modulo = "Colaborador",
                            Titulo = x.Nombre + " " + x.Apellido,
                            Subtitulo = x.Rol,
                            Controller = "Colaboradores",
                            Action = "Details",
                            Id = x.IdColaborador
                        })
                        .ToListAsync();

                    var validaciones = await _context.Validaciones
                        .AsNoTracking()
                        .Where(x => (x.Resultado != null && x.Resultado.Contains(term)) || (x.Validador != null && x.Validador.Contains(term)))
                        .OrderByDescending(x => x.Fecha)
                        .Take(top)
                        .Select(x => new SearchHitViewModel
                        {
                            Modulo = "Validacion",
                            Titulo = x.Resultado ?? "Sin resultado",
                            Subtitulo = x.Validador,
                            Controller = "Validaciones",
                            Action = "Details",
                            Id = x.IdValidacion
                        })
                        .ToListAsync();

                    hits.AddRange(experimentos);
                    hits.AddRange(publicaciones);
                    hits.AddRange(repos);
                    hits.AddRange(colaboradores);
                    hits.AddRange(validaciones);
                }
            }

            return View(new GlobalSearchViewModel
            {
                Query = q,
                Hits = hits
                    .OrderBy(h => h.Modulo)
                    .ThenBy(h => h.Titulo)
                    .ToList()
            });
        }
    }
}
