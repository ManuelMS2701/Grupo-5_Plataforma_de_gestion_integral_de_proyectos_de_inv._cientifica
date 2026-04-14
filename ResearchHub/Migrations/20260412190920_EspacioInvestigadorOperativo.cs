using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchHub.Migrations
{
    /// <inheritdoc />
    public partial class EspacioInvestigadorOperativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BitacoraProyecto",
                columns: table => new
                {
                    IdBitacora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdProyecto = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraProyecto", x => x.IdBitacora);
                    table.ForeignKey(
                        name: "FK_BitacoraProyecto_Proyectos_IdProyecto",
                        column: x => x.IdProyecto,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BitacoraProyecto_UsuariosSistema_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "UsuariosSistema",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SeguimientosMuestra",
                columns: table => new
                {
                    IdSeguimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IdMuestra = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientosMuestra", x => x.IdSeguimiento);
                    table.ForeignKey(
                        name: "FK_SeguimientosMuestra_Muestras_IdMuestra",
                        column: x => x.IdMuestra,
                        principalTable: "Muestras",
                        principalColumn: "IdMuestra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeguimientosMuestra_UsuariosSistema_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "UsuariosSistema",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TareasInvestigacion",
                columns: table => new
                {
                    IdTarea = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdProyecto = table.Column<int>(type: "int", nullable: false),
                    IdExperimento = table.Column<int>(type: "int", nullable: true),
                    IdResponsable = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareasInvestigacion", x => x.IdTarea);
                    table.ForeignKey(
                        name: "FK_TareasInvestigacion_Experimentos_IdExperimento",
                        column: x => x.IdExperimento,
                        principalTable: "Experimentos",
                        principalColumn: "IdExperimento",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TareasInvestigacion_Investigadores_IdResponsable",
                        column: x => x.IdResponsable,
                        principalTable: "Investigadores",
                        principalColumn: "IdInvestigador",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TareasInvestigacion_Proyectos_IdProyecto",
                        column: x => x.IdProyecto,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraProyecto_IdProyecto",
                table: "BitacoraProyecto",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraProyecto_IdUsuario",
                table: "BitacoraProyecto",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientosMuestra_IdMuestra",
                table: "SeguimientosMuestra",
                column: "IdMuestra");

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientosMuestra_IdUsuario",
                table: "SeguimientosMuestra",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_TareasInvestigacion_IdExperimento",
                table: "TareasInvestigacion",
                column: "IdExperimento");

            migrationBuilder.CreateIndex(
                name: "IX_TareasInvestigacion_IdProyecto",
                table: "TareasInvestigacion",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_TareasInvestigacion_IdResponsable",
                table: "TareasInvestigacion",
                column: "IdResponsable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacoraProyecto");

            migrationBuilder.DropTable(
                name: "SeguimientosMuestra");

            migrationBuilder.DropTable(
                name: "TareasInvestigacion");
        }
    }
}
