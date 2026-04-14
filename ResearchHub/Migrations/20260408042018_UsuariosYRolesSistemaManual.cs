using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchHub.Migrations
{
    /// <inheritdoc />
    public partial class UsuariosYRolesSistemaManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolesSistema",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesSistema", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSistema",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    BloqueadoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    IdInvestigador = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSistema", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_UsuariosSistema_Investigadores_IdInvestigador",
                        column: x => x.IdInvestigador,
                        principalTable: "Investigadores",
                        principalColumn: "IdInvestigador",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsuariosSistema_RolesSistema_IdRol",
                        column: x => x.IdRol,
                        principalTable: "RolesSistema",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolesSistema_Nombre",
                table: "RolesSistema",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_Email",
                table: "UsuariosSistema",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_IdInvestigador",
                table: "UsuariosSistema",
                column: "IdInvestigador");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_IdRol",
                table: "UsuariosSistema",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_NombreUsuario",
                table: "UsuariosSistema",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Administrador')
BEGIN
    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
    VALUES ('Administrador', 'Control total del sistema y de la configuración científica.', 1);
END;

IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Usuario')
BEGIN
    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
    VALUES ('Usuario', 'Investigador con acceso operativo a su espacio de trabajo.', 1);
END;

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
SELECT
    COALESCE(NULLIF(LEFT(COALESCE(u.UserName, u.Email), CHARINDEX('@', COALESCE(u.UserName, u.Email) + '@') - 1), ''), COALESCE(u.Email, 'usuario')),
    COALESCE(u.UserName, u.Email),
    COALESCE(u.Email, u.UserName),
    COALESCE(u.PasswordHash, ''),
    CAST(1 AS bit),
    GETUTCDATE(),
    NULL,
    0,
    NULL,
    COALESCE(rs.IdRol, (SELECT TOP 1 IdRol FROM RolesSistema WHERE Nombre = 'Usuario')),
    inv.IdInvestigador
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
LEFT JOIN AspNetRoles ar ON ar.Id = ur.RoleId
LEFT JOIN RolesSistema rs ON rs.Nombre = ar.Name
LEFT JOIN Investigadores inv ON LOWER(inv.Email) = LOWER(COALESCE(u.Email, u.UserName))
WHERE COALESCE(u.Email, u.UserName) IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM UsuariosSistema us
      WHERE us.Email = COALESCE(u.Email, u.UserName)
  );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuariosSistema");

            migrationBuilder.DropTable(
                name: "RolesSistema");
        }
    }
}
