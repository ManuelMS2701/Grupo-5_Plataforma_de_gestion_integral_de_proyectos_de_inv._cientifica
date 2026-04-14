using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Models;

namespace ResearchHub.Data
{
    public static class DemoUsersSeeder
    {
        public static async Task SeedAsync(ResearchHubContext context)
        {
            var specs = new[]
            {
                new DemoUserSpec("Ana Rodriguez", "ana.rodriguez", "ana.rodriguez@cnba.edu.do", Roles.InvestigadorPrincipal, "AnaRH2026!"),
                new DemoUserSpec("Carla Perez", "carla.perez", "carla.perez@uta.edu.do", Roles.AnalistaDatos, "CarlaRH2026!"),
                new DemoUserSpec("Miguel Santos", "miguel.santos", "miguel.santos@cnba.edu.do", Roles.TecnicoLaboratorio, "MiguelRH2026!"),
                new DemoUserSpec("Elena Gomez", "elena.gomez", "elena.gomez@icst.edu.do", Roles.RevisorCientifico, "ElenaRH2026!"),
                new DemoUserSpec("Luis Martinez", "luis.martinez", "luis.martinez@icst.edu.do", Roles.ColaboradorExterno, "LuisRH2026!")
            };

            var hasher = new PasswordHasher<UsuarioSistema>();

            foreach (var spec in specs)
            {
                var rol = await context.RolesSistema.FirstOrDefaultAsync(r => r.Nombre == spec.Rol && r.Activo);
                if (rol == null)
                {
                    continue;
                }

                var usuario = await context.UsuariosSistema.FirstOrDefaultAsync(u => u.Email == spec.Email);
                if (usuario != null)
                {
                    if (usuario.IdRol != rol.IdRol)
                    {
                        usuario.IdRol = rol.IdRol;
                    }

                    if (!usuario.IdInvestigador.HasValue)
                    {
                        usuario.IdInvestigador = await context.Investigadores
                            .Where(i => i.Email == spec.Email)
                            .Select(i => (int?)i.IdInvestigador)
                            .FirstOrDefaultAsync();
                    }

                    continue;
                }

                usuario = new UsuarioSistema
                {
                    NombreCompleto = spec.NombreCompleto,
                    NombreUsuario = spec.NombreUsuario,
                    Email = spec.Email,
                    IdRol = rol.IdRol,
                    IdInvestigador = await context.Investigadores
                        .Where(i => i.Email == spec.Email)
                        .Select(i => (int?)i.IdInvestigador)
                        .FirstOrDefaultAsync(),
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                };

                usuario.PasswordHash = hasher.HashPassword(usuario, spec.Password);
                context.UsuariosSistema.Add(usuario);
            }

            await context.SaveChangesAsync();
        }

        private sealed record DemoUserSpec(string NombreCompleto, string NombreUsuario, string Email, string Rol, string Password);
    }
}
