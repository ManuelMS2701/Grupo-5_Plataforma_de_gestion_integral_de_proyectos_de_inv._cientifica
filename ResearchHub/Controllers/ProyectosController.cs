using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Data;
using ResearchHub.Models;

namespace ResearchHub.Controllers
{
    [Authorize]
    public class ProyectosController : Controller
    {
        private enum NivelAcceso
        {
            Ninguno = 0,
            Read = 1,
            Write = 2,
            Admin = 3
        }

        private readonly ResearchHubContext _context;

        public ProyectosController(ResearchHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? estado, bool misProyectos = false)
        {
            var isAdmin = User.IsInRole(Roles.Administrador);
            var userEmail = User.Identity?.Name;

            var query = _context.Proyectos
                .Include(p => p.Institucion)
                .Include(p => p.InvestigadorPrincipal)
                .Include(p => p.LineaInvestigacion)
                .Include(p => p.SublineaInvestigacion)
                .AsNoTracking()
                .AsQueryable();

            if (misProyectos)
            {
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    query = query.Where(_ => false);
                }
                else
                {
                    query = query.Where(p =>
                        (p.InvestigadorPrincipal != null && p.InvestigadorPrincipal.Email == userEmail) ||
                        p.Colaboradores.Any(c => c.Email == userEmail));
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Titulo.Contains(term) ||
                    (p.Descripcion != null && p.Descripcion.Contains(term)) ||
                    (p.Institucion != null && p.Institucion.Nombre.Contains(term)) ||
                    (p.SublineaInvestigacion != null && p.SublineaInvestigacion.Nombre.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(p => p.Estado == estado);
            }

            var proyectos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            var permisos = await ConstruirPermisosAsync(proyectos, userEmail, isAdmin);

            ViewData["Search"] = search;
            ViewData["Estado"] = estado;
            ViewData["MisProyectos"] = misProyectos;
            ViewData["PuedeCrearProyecto"] = isAdmin;
            ViewData["PermisosProyecto"] = permisos;
            ViewData["Estados"] = await _context.Proyectos
                .AsNoTracking()
                .Where(p => !string.IsNullOrWhiteSpace(p.Estado))
                .Select(p => p.Estado!)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            return View(proyectos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos
                .Include(p => p.Institucion)
                .Include(p => p.InvestigadorPrincipal)
                .Include(p => p.LineaInvestigacion)
                .Include(p => p.SublineaInvestigacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProyecto == id.Value);

            if (proyecto == null) return NotFound();

            var isAdmin = User.IsInRole(Roles.Administrador);
            var nivel = await ObtenerNivelAccesoAsync(id.Value, User.Identity?.Name, isAdmin);
            if (nivel == NivelAcceso.Ninguno)
            {
                return Forbid();
            }

            ViewData["NivelAcceso"] = NivelComoTexto(nivel);
            ViewData["PuedeEditarProyecto"] = nivel >= NivelAcceso.Write || isAdmin;

            return View(proyecto);
        }

        public async Task<IActionResult> Hub(int id)
        {
            var proyecto = await _context.Proyectos
                .Include(p => p.Institucion)
                .Include(p => p.InvestigadorPrincipal)
                .Include(p => p.LineaInvestigacion)
                .Include(p => p.SublineaInvestigacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProyecto == id);

            if (proyecto == null) return NotFound();

            var isAdmin = User.IsInRole(Roles.Administrador);
            var nivel = await ObtenerNivelAccesoAsync(id, User.Identity?.Name, isAdmin);
            if (nivel == NivelAcceso.Ninguno)
            {
                return Forbid();
            }

            var experimentoIds = await _context.Experimentos
                .AsNoTracking()
                .Where(e => e.IdProyecto == id)
                .Select(e => e.IdExperimento)
                .ToListAsync();

            var resultadoIds = await _context.Resultados
                .AsNoTracking()
                .Where(r => experimentoIds.Contains(r.IdExperimento))
                .Select(r => r.IdResultado)
                .ToListAsync();

            var analisisIds = await _context.Analisis
                .AsNoTracking()
                .Where(a => resultadoIds.Contains(a.IdResultado))
                .Select(a => a.IdAnalisis)
                .ToListAsync();

            var totalExperimentos = experimentoIds.Count;
            var totalMuestras = await _context.Muestras.AsNoTracking().CountAsync(m => m.IdProyecto == id);
            var totalResultados = resultadoIds.Count;
            var totalAnalisis = analisisIds.Count;
            var totalValidaciones = await _context.Validaciones.AsNoTracking().CountAsync(v => analisisIds.Contains(v.IdAnalisis));
            var totalPublicaciones = await _context.Publicaciones.AsNoTracking().CountAsync(p => p.IdProyecto == id);
            var totalRepositorios = await _context.RepositoriosDatos.AsNoTracking().CountAsync(r => r.IdProyecto == id);
            var totalColaboradores = await _context.Colaboradores.AsNoTracking().CountAsync(c => c.IdProyecto == id);
            var totalCronograma = await _context.Cronogramas.AsNoTracking().CountAsync(c => c.IdProyecto == id);

            var hitosPendientes = await _context.Cronogramas.AsNoTracking()
                .CountAsync(c => c.IdProyecto == id && c.EsHito && (c.Estado == null || !c.Estado.ToLower().Contains("complet")));
            var hitosCompletados = await _context.Cronogramas.AsNoTracking()
                .CountAsync(c => c.IdProyecto == id && c.EsHito && c.Estado != null && c.Estado.ToLower().Contains("complet"));

            var proximosHitos = await _context.Cronogramas
                .AsNoTracking()
                .Where(c => c.IdProyecto == id && c.EsHito)
                .OrderBy(c => c.FechaFin ?? c.FechaInicio)
                .Take(5)
                .ToListAsync();

            var eventos = new List<HubEventoItem>();

            var eventosResultados = await _context.Resultados
                .AsNoTracking()
                .Include(r => r.Experimento)
                .Where(r => experimentoIds.Contains(r.IdExperimento))
                .OrderByDescending(r => r.FechaRegistro)
                .Take(4)
                .Select(r => new HubEventoItem
                {
                    Tipo = "Resultado",
                    Titulo = $"Resultado #{r.IdResultado}",
                    Fecha = r.FechaRegistro,
                    Detalle = r.Experimento != null ? r.Experimento.Titulo : null
                })
                .ToListAsync();

            var eventosPublicaciones = await _context.Publicaciones
                .AsNoTracking()
                .Where(p => p.IdProyecto == id && p.FechaPublicacion != null)
                .OrderByDescending(p => p.FechaPublicacion)
                .Take(4)
                .Select(p => new HubEventoItem
                {
                    Tipo = "Publicación",
                    Titulo = p.Titulo,
                    Fecha = p.FechaPublicacion ?? DateTime.MinValue,
                    Detalle = p.Revista
                })
                .ToListAsync();

            var eventosValidaciones = await _context.Validaciones
                .AsNoTracking()
                .Where(v => analisisIds.Contains(v.IdAnalisis) && v.Fecha != null)
                .OrderByDescending(v => v.Fecha)
                .Take(4)
                .Select(v => new HubEventoItem
                {
                    Tipo = "Validación",
                    Titulo = v.Resultado ?? "Validación",
                    Fecha = v.Fecha ?? DateTime.MinValue,
                    Detalle = v.Validador
                })
                .ToListAsync();

            var eventosCronograma = await _context.Cronogramas
                .AsNoTracking()
                .Where(c => c.IdProyecto == id && c.FechaInicio != null)
                .OrderByDescending(c => c.FechaInicio)
                .Take(4)
                .Select(c => new HubEventoItem
                {
                    Tipo = "Cronograma",
                    Titulo = c.NombreFase,
                    Fecha = c.FechaInicio ?? DateTime.MinValue,
                    Detalle = c.Estado
                })
                .ToListAsync();

            eventos.AddRange(eventosResultados);
            eventos.AddRange(eventosPublicaciones);
            eventos.AddRange(eventosValidaciones);
            eventos.AddRange(eventosCronograma);

            var vm = new ProyectoHubViewModel
            {
                Proyecto = proyecto,
                NivelAcceso = NivelComoTexto(nivel),
                TotalExperimentos = totalExperimentos,
                TotalMuestras = totalMuestras,
                TotalResultados = totalResultados,
                TotalAnalisis = totalAnalisis,
                TotalValidaciones = totalValidaciones,
                TotalPublicaciones = totalPublicaciones,
                TotalRepositorios = totalRepositorios,
                TotalColaboradores = totalColaboradores,
                TotalCronograma = totalCronograma,
                HitosPendientes = hitosPendientes,
                HitosCompletados = hitosCompletados,
                ProximosHitos = proximosHitos,
                EventosRecientes = eventos.OrderByDescending(e => e.Fecha).Take(12).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Relacionados(int id, string tipo)
        {
            var proyecto = await _context.Proyectos
                .Include(p => p.LineaInvestigacion)
                .Include(p => p.SublineaInvestigacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProyecto == id);

            if (proyecto == null) return NotFound();

            var isAdmin = User.IsInRole(Roles.Administrador);
            var nivel = await ObtenerNivelAccesoAsync(id, User.Identity?.Name, isAdmin);
            if (nivel == NivelAcceso.Ninguno)
            {
                return Forbid();
            }

            var key = (tipo ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(key))
            {
                return RedirectToAction(nameof(Hub), new { id });
            }

            ProyectoRelacionadosViewModel? vm = key switch
            {
                "linea" => BuildLineaRelacionada(proyecto),
                "sublinea" => BuildSublineaRelacionada(proyecto),
                "experimentos" => await BuildExperimentosRelacionadosAsync(proyecto),
                "muestras" => await BuildMuestrasRelacionadasAsync(proyecto),
                "resultados" => await BuildResultadosRelacionadosAsync(proyecto),
                "analisis" => await BuildAnalisisRelacionadosAsync(proyecto),
                "validaciones" => await BuildValidacionesRelacionadasAsync(proyecto),
                "publicaciones" => await BuildPublicacionesRelacionadasAsync(proyecto),
                "repositorios" => await BuildRepositoriosRelacionadosAsync(proyecto),
                "colaboradores" => await BuildColaboradoresRelacionadosAsync(proyecto),
                "cronograma" => await BuildCronogramaRelacionadoAsync(proyecto),
                _ => null
            };

            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Create()
        {
            ViewData["ModoInvestigador"] = false;
            await CargarCombosAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Create(Proyecto proyecto)
        {
            ValidarSublinea(proyecto);

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(proyecto.IdLinea, proyecto.IdSublinea);
                return View(proyecto);
            }

            proyecto.FechaCreacion = DateTime.UtcNow;
            _context.Proyectos.Add(proyecto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Usuario + "," + Roles.Administrador)]
        public async Task<IActionResult> CreatePropio()
        {
            var investigador = await BuscarInvestigadorActualAsync();
            if (investigador == null)
            {
                return Forbid();
            }

            ViewData["ModoInvestigador"] = true;
            ViewData["InvestigadorActual"] = investigador.Nombre + " " + investigador.Apellido;
            ViewData["InstitucionActual"] = investigador.Institucion?.Nombre ?? "Sin institución";

            await CargarCombosAsync();
            return View("Create", new Proyecto
            {
                IdInvestigadorPrincipal = investigador.IdInvestigador,
                IdInstitucion = investigador.IdInstitucion
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Usuario + "," + Roles.Administrador)]
        public async Task<IActionResult> CreatePropio(Proyecto proyecto)
        {
            var investigador = await BuscarInvestigadorActualAsync();
            if (investigador == null)
            {
                return Forbid();
            }

            proyecto.IdInvestigadorPrincipal = investigador.IdInvestigador;
            proyecto.IdInstitucion = investigador.IdInstitucion;

            ValidarSublinea(proyecto);

            if (!ModelState.IsValid)
            {
                ViewData["ModoInvestigador"] = true;
                ViewData["InvestigadorActual"] = investigador.Nombre + " " + investigador.Apellido;
                ViewData["InstitucionActual"] = investigador.Institucion?.Nombre ?? "Sin institución";
                await CargarCombosAsync(proyecto.IdLinea, proyecto.IdSublinea);
                return View("Create", proyecto);
            }

            proyecto.FechaCreacion = DateTime.UtcNow;
            _context.Proyectos.Add(proyecto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { misProyectos = true });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var isAdmin = User.IsInRole(Roles.Administrador);
            var nivel = await ObtenerNivelAccesoAsync(id.Value, User.Identity?.Name, isAdmin);
            if (!(isAdmin || nivel >= NivelAcceso.Write))
            {
                return Forbid();
            }

            var proyecto = await _context.Proyectos.FindAsync(id.Value);
            if (proyecto == null) return NotFound();

            await CargarCombosAsync(proyecto.IdLinea, proyecto.IdSublinea);
            return View(proyecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Proyecto proyecto)
        {
            if (id != proyecto.IdProyecto) return NotFound();

            var isAdmin = User.IsInRole(Roles.Administrador);
            var nivel = await ObtenerNivelAccesoAsync(id, User.Identity?.Name, isAdmin);
            if (!(isAdmin || nivel >= NivelAcceso.Write))
            {
                return Forbid();
            }

            ValidarSublinea(proyecto);

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(proyecto.IdLinea, proyecto.IdSublinea);
                return View(proyecto);
            }

            try
            {
                _context.Update(proyecto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProyectoExisteAsync(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos
                .Include(p => p.Institucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProyecto == id.Value);

            if (proyecto == null) return NotFound();

            return View(proyecto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Dictionary<int, string>> ConstruirPermisosAsync(IEnumerable<Proyecto> proyectos, string? userEmail, bool isAdmin)
        {
            var salida = new Dictionary<int, string>();

            foreach (var proyecto in proyectos)
            {
                var nivel = await ObtenerNivelAccesoAsync(proyecto.IdProyecto, userEmail, isAdmin);
                salida[proyecto.IdProyecto] = NivelComoTexto(nivel);
            }

            return salida;
        }

        private async Task<NivelAcceso> ObtenerNivelAccesoAsync(int idProyecto, string? userEmail, bool isAdmin)
        {
            if (isAdmin)
            {
                return NivelAcceso.Admin;
            }

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return NivelAcceso.Ninguno;
            }

            var info = await _context.Proyectos
                .AsNoTracking()
                .Where(p => p.IdProyecto == idProyecto)
                .Select(p => new
                {
                    PrincipalEmail = p.InvestigadorPrincipal != null ? p.InvestigadorPrincipal.Email : null,
                    RolColaborador = p.Colaboradores
                        .Where(c => c.Email == userEmail)
                        .Select(c => c.Rol)
                        .FirstOrDefault(),
                    EsColaborador = p.Colaboradores.Any(c => c.Email == userEmail)
                })
                .FirstOrDefaultAsync();

            if (info == null)
            {
                return NivelAcceso.Ninguno;
            }

            if (!string.IsNullOrWhiteSpace(info.PrincipalEmail) &&
                string.Equals(info.PrincipalEmail, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return NivelAcceso.Admin;
            }

            if (info.EsColaborador)
            {
                return ParsearRolColaborador(info.RolColaborador);
            }

            return NivelAcceso.Ninguno;
        }

        private static NivelAcceso ParsearRolColaborador(string? rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
            {
                return NivelAcceso.Read;
            }

            var valor = rol.Trim().ToLowerInvariant();
            if (valor.Contains("admin")) return NivelAcceso.Admin;
            if (valor.Contains("write") || valor.Contains("editor") || valor.Contains("edicion") || valor.Contains("editar")) return NivelAcceso.Write;
            if (valor.Contains("read") || valor.Contains("lectura") || valor.Contains("lector") || valor.Contains("ver")) return NivelAcceso.Read;

            return NivelAcceso.Read;
        }

        private static string NivelComoTexto(NivelAcceso nivel)
        {
            return nivel switch
            {
                NivelAcceso.Admin => "Admin",
                NivelAcceso.Write => "Write",
                NivelAcceso.Read => "Read",
                _ => "Ninguno"
            };
        }

        private void ValidarSublinea(Proyecto proyecto)
        {
            if (!proyecto.IdSublinea.HasValue)
            {
                return;
            }

            var esValida = _context.SublineasInvestigacion.Any(s =>
                s.IdSublinea == proyecto.IdSublinea.Value &&
                s.IdLinea == proyecto.IdLinea &&
                s.Activa);

            if (!esValida)
            {
                ModelState.AddModelError(nameof(Proyecto.IdSublinea), "La sublínea seleccionada no pertenece a la línea de investigación elegida.");
            }
        }

        private async Task CargarCombosAsync(int? idLinea = null, int? idSublinea = null)
        {
            ViewData["Investigadores"] = new SelectList(
                await _context.Investigadores
                    .AsNoTracking()
                    .OrderBy(i => i.Apellido)
                    .ThenBy(i => i.Nombre)
                    .Select(i => new { i.IdInvestigador, Nombre = i.Nombre + " " + i.Apellido })
                    .ToListAsync(),
                "IdInvestigador",
                "Nombre");

            ViewData["Instituciones"] = new SelectList(
                await _context.Instituciones.AsNoTracking().OrderBy(i => i.Nombre).ToListAsync(),
                "IdInstitucion",
                "Nombre");

            ViewData["Lineas"] = new SelectList(
                await _context.LineasInvestigacion.AsNoTracking().OrderBy(l => l.Nombre).ToListAsync(),
                "IdLinea",
                "Nombre",
                idLinea);

            var sublineas = _context.SublineasInvestigacion
                .AsNoTracking()
                .Where(s => s.Activa)
                .Include(s => s.LineaInvestigacion)
                .AsQueryable();

            if (idLinea.HasValue)
            {
                sublineas = sublineas.Where(s => s.IdLinea == idLinea.Value);
            }

            ViewData["Sublineas"] = new SelectList(
                await sublineas
                    .OrderBy(s => s.Nombre)
                    .Select(s => new
                    {
                        s.IdSublinea,
                        Nombre = s.LineaInvestigacion != null ? s.LineaInvestigacion.Nombre + " / " + s.Nombre : s.Nombre
                    })
                    .ToListAsync(),
                "IdSublinea",
                "Nombre",
                idSublinea);
        }

        private async Task<Investigador?> BuscarInvestigadorActualAsync()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return null;
            }

            return await _context.Investigadores
                .Include(i => i.Institucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Email == userEmail);
        }

        private async Task<bool> ProyectoExisteAsync(int id)
        {
            return await _context.Proyectos.AnyAsync(e => e.IdProyecto == id);
        }

        private ProyectoRelacionadosViewModel BuildLineaRelacionada(Proyecto proyecto)
        {
            var filas = new List<ProyectoRelacionadoRowViewModel>();
            if (proyecto.LineaInvestigacion != null)
            {
                filas.Add(new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { proyecto.LineaInvestigacion.Nombre, proyecto.LineaInvestigacion.Descripcion ?? string.Empty, proyecto.LineaInvestigacion.Activa ? "Sí" : "No" },
                    DetalleController = "LineasInvestigacion",
                    DetalleId = proyecto.LineaInvestigacion.IdLinea
                });
            }

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "linea",
                Titulo = "Línea de investigación",
                Columnas = new[] { "Nombre", "Descripción", "Activa" },
                Filas = filas
            };
        }

        private ProyectoRelacionadosViewModel BuildSublineaRelacionada(Proyecto proyecto)
        {
            var filas = new List<ProyectoRelacionadoRowViewModel>();
            if (proyecto.SublineaInvestigacion != null)
            {
                filas.Add(new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { proyecto.SublineaInvestigacion.Nombre, proyecto.SublineaInvestigacion.Descripcion ?? string.Empty, proyecto.SublineaInvestigacion.Activa ? "Sí" : "No" },
                    DetalleController = "SublineasInvestigacion",
                    DetalleId = proyecto.SublineaInvestigacion.IdSublinea
                });
            }

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "sublinea",
                Titulo = "Sublínea de investigación",
                Columnas = new[] { "Nombre", "Descripción", "Activa" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildExperimentosRelacionadosAsync(Proyecto proyecto)
        {
            var filas = await _context.Experimentos
                .AsNoTracking()
                .Where(e => e.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(e => e.FechaInicio)
                .Select(e => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { e.Titulo, e.Estado ?? string.Empty, e.FechaInicio.HasValue ? e.FechaInicio.Value.ToString("yyyy-MM-dd") : string.Empty },
                    DetalleController = "Experimentos",
                    DetalleId = e.IdExperimento
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "experimentos",
                Titulo = "Experimentos del proyecto",
                Columnas = new[] { "Título", "Estado", "Inicio" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildMuestrasRelacionadasAsync(Proyecto proyecto)
        {
            var filas = await _context.Muestras
                .AsNoTracking()
                .Where(m => m.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(m => m.FechaRecoleccion)
                .Select(m => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { m.Codigo, m.Tipo ?? string.Empty, m.FechaRecoleccion.HasValue ? m.FechaRecoleccion.Value.ToString("yyyy-MM-dd") : string.Empty },
                    DetalleController = "Muestras",
                    DetalleId = m.IdMuestra
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "muestras",
                Titulo = "Muestras del proyecto",
                Columnas = new[] { "Código", "Tipo", "Fecha" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildResultadosRelacionadosAsync(Proyecto proyecto)
        {
            var filas = await _context.Resultados
                .Include(r => r.Experimento)
                .AsNoTracking()
                .Where(r => r.Experimento != null && r.Experimento.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(r => r.FechaRegistro)
                .Select(r => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { $"Resultado #{r.IdResultado}", r.Experimento != null ? r.Experimento.Titulo : string.Empty, r.Valor ?? string.Empty, r.FechaRegistro.ToString("yyyy-MM-dd") },
                    DetalleController = "Resultados",
                    DetalleId = r.IdResultado
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "resultados",
                Titulo = "Resultados del proyecto",
                Columnas = new[] { "Registro", "Experimento", "Valor", "Fecha" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildAnalisisRelacionadosAsync(Proyecto proyecto)
        {
            var filas = await _context.Analisis
                .Include(a => a.Resultado)
                    .ThenInclude(r => r!.Experimento)
                .AsNoTracking()
                .Where(a => a.Resultado != null && a.Resultado.Experimento != null && a.Resultado.Experimento.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(a => a.Fecha)
                .Select(a => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { a.Titulo, a.Metodo ?? string.Empty, a.Fecha.HasValue ? a.Fecha.Value.ToString("yyyy-MM-dd") : string.Empty },
                    DetalleController = "Analisis",
                    DetalleId = a.IdAnalisis
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "analisis",
                Titulo = "Análisis del proyecto",
                Columnas = new[] { "Título", "Método", "Fecha" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildValidacionesRelacionadasAsync(Proyecto proyecto)
        {
            var filas = await _context.Validaciones
                .Include(v => v.Analisis)
                    .ThenInclude(a => a!.Resultado)
                        .ThenInclude(r => r!.Experimento)
                .AsNoTracking()
                .Where(v => v.Analisis != null && v.Analisis.Resultado != null && v.Analisis.Resultado.Experimento != null && v.Analisis.Resultado.Experimento.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(v => v.Fecha)
                .Select(v => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { v.Resultado ?? string.Empty, v.Validador ?? string.Empty, v.Fecha.HasValue ? v.Fecha.Value.ToString("yyyy-MM-dd") : string.Empty },
                    DetalleController = "Validaciones",
                    DetalleId = v.IdValidacion
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "validaciones",
                Titulo = "Validaciones del proyecto",
                Columnas = new[] { "Resultado", "Validador", "Fecha" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildPublicacionesRelacionadasAsync(Proyecto proyecto)
        {
            var filas = await _context.Publicaciones
                .AsNoTracking()
                .Where(p => p.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(p => p.FechaPublicacion)
                .Select(p => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { p.Titulo, p.Revista ?? string.Empty, p.FechaPublicacion.HasValue ? p.FechaPublicacion.Value.ToString("yyyy-MM-dd") : string.Empty },
                    DetalleController = "Publicaciones",
                    DetalleId = p.IdPublicacion
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "publicaciones",
                Titulo = "Publicaciones del proyecto",
                Columnas = new[] { "Título", "Revista", "Fecha" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildRepositoriosRelacionadosAsync(Proyecto proyecto)
        {
            var filas = await _context.RepositoriosDatos
                .AsNoTracking()
                .Where(r => r.IdProyecto == proyecto.IdProyecto)
                .OrderByDescending(r => r.FechaRegistro)
                .Select(r => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { r.Nombre, r.Tipo ?? string.Empty, r.Url ?? string.Empty },
                    DetalleController = "RepositoriosDatos",
                    DetalleId = r.IdRepositorio
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "repositorios",
                Titulo = "Repositorios del proyecto",
                Columnas = new[] { "Nombre", "Tipo", "URL" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildColaboradoresRelacionadosAsync(Proyecto proyecto)
        {
            var filas = await _context.Colaboradores
                .AsNoTracking()
                .Where(c => c.IdProyecto == proyecto.IdProyecto)
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Select(c => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { c.Nombre + " " + c.Apellido, c.Rol ?? string.Empty, c.Email ?? string.Empty },
                    DetalleController = "Colaboradores",
                    DetalleId = c.IdColaborador
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "colaboradores",
                Titulo = "Colaboradores del proyecto",
                Columnas = new[] { "Nombre", "Rol", "Email" },
                Filas = filas
            };
        }

        private async Task<ProyectoRelacionadosViewModel> BuildCronogramaRelacionadoAsync(Proyecto proyecto)
        {
            var filas = await _context.Cronogramas
                .Include(c => c.Dependencia)
                .AsNoTracking()
                .Where(c => c.IdProyecto == proyecto.IdProyecto)
                .OrderBy(c => c.FechaInicio)
                .ThenBy(c => c.NombreFase)
                .Select(c => new ProyectoRelacionadoRowViewModel
                {
                    Valores = new[] { c.NombreFase, c.EsHito ? "Sí" : "No", c.Estado ?? string.Empty, $"{c.PorcentajeAvance}%" },
                    DetalleController = "Cronogramas",
                    DetalleId = c.IdCronograma
                })
                .ToListAsync();

            return new ProyectoRelacionadosViewModel
            {
                Proyecto = proyecto,
                Tipo = "cronograma",
                Titulo = "Cronograma del proyecto",
                Columnas = new[] { "Fase", "Hito", "Estado", "Avance" },
                Filas = filas
            };
        }
    }
}


