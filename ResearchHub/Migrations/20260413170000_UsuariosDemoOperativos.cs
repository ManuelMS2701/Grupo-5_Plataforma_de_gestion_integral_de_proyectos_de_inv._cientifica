using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ResearchHub.Models;

#nullable disable

namespace ResearchHub.Migrations
{
    [DbContext(typeof(ResearchHub.Data.ResearchHubContext))]
    [Migration("20260413170000_UsuariosDemoOperativos")]
    public class UsuariosDemoOperativos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            InsertarUsuarioDemo(migrationBuilder, "Ana Rodriguez", "ana.rodriguez", "ana.rodriguez@cnba.edu.do", Roles.InvestigadorPrincipal, "ana.rodriguez@cnba.edu.do", "AnaRH2026!");
            InsertarUsuarioDemo(migrationBuilder, "Carla Perez", "carla.perez", "carla.perez@uta.edu.do", Roles.AnalistaDatos, "carla.perez@uta.edu.do", "CarlaRH2026!");
            InsertarUsuarioDemo(migrationBuilder, "Miguel Santos", "miguel.santos", "miguel.santos@cnba.edu.do", Roles.TecnicoLaboratorio, "miguel.santos@cnba.edu.do", "MiguelRH2026!");
            InsertarUsuarioDemo(migrationBuilder, "Elena Gomez", "elena.gomez", "elena.gomez@icst.edu.do", Roles.RevisorCientifico, "elena.gomez@icst.edu.do", "ElenaRH2026!");
            InsertarUsuarioDemo(migrationBuilder, "Luis Martinez", "luis.martinez", "luis.martinez@icst.edu.do", Roles.ColaboradorExterno, "luis.martinez@icst.edu.do", "LuisRH2026!");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM UsuariosSistema
                WHERE Email IN (
                    'ana.rodriguez@cnba.edu.do',
                    'carla.perez@uta.edu.do',
                    'miguel.santos@cnba.edu.do',
                    'elena.gomez@icst.edu.do',
                    'luis.martinez@icst.edu.do'
                );
                """);
        }

        private static void InsertarUsuarioDemo(MigrationBuilder migrationBuilder, string nombreCompleto, string nombreUsuario, string email, string rol, string investigadorEmail, string password)
        {
            var usuario = new UsuarioSistema
            {
                NombreCompleto = nombreCompleto,
                NombreUsuario = nombreUsuario,
                Email = email,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            var hasher = new PasswordHasher<UsuarioSistema>();
            var passwordHash = hasher.HashPassword(usuario, password).Replace("'", "''");
            var nombreCompletoSql = nombreCompleto.Replace("'", "''");
            var nombreUsuarioSql = nombreUsuario.Replace("'", "''");
            var emailSql = email.Replace("'", "''");
            var rolSql = rol.Replace("'", "''");
            var investigadorEmailSql = investigadorEmail.Replace("'", "''");

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM UsuariosSistema WHERE Email = '{emailSql}')
                BEGIN
                    DECLARE @IdRol INT = (SELECT TOP 1 IdRol FROM RolesSistema WHERE Nombre = '{rolSql}');
                    DECLARE @IdInvestigador INT = (SELECT TOP 1 IdInvestigador FROM Investigadores WHERE Email = '{investigadorEmailSql}');

                    IF @IdRol IS NOT NULL
                    BEGIN
                        INSERT INTO UsuariosSistema
                        (
                            NombreCompleto,
                            NombreUsuario,
                            Email,
                            PasswordHash,
                            Activo,
                            FechaRegistro,
                            UltimoAcceso,
                            IntentosFallidos,
                            BloqueadoHasta,
                            IdRol,
                            IdInvestigador
                        )
                        VALUES
                        (
                            '{nombreCompletoSql}',
                            '{nombreUsuarioSql}',
                            '{emailSql}',
                            '{passwordHash}',
                            1,
                            SYSUTCDATETIME(),
                            NULL,
                            0,
                            NULL,
                            @IdRol,
                            @IdInvestigador
                        );
                    END
                END;
                """);
        }
    }
}
