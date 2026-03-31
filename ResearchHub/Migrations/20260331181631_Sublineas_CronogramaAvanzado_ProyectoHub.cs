using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchHub.Migrations
{
    /// <inheritdoc />
    public partial class Sublineas_CronogramaAvanzado_ProyectoHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdSublinea",
                table: "Proyectos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsHito",
                table: "Cronogramas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdDependencia",
                table: "Cronogramas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PorcentajeAvance",
                table: "Cronogramas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Cronogramas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsable",
                table: "Cronogramas",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Riesgo",
                table: "Cronogramas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SublineasInvestigacion",
                columns: table => new
                {
                    IdSublinea = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    IdLinea = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SublineasInvestigacion", x => x.IdSublinea);
                    table.ForeignKey(
                        name: "FK_SublineasInvestigacion_LineasInvestigacion_IdLinea",
                        column: x => x.IdLinea,
                        principalTable: "LineasInvestigacion",
                        principalColumn: "IdLinea",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_IdSublinea",
                table: "Proyectos",
                column: "IdSublinea");

            migrationBuilder.CreateIndex(
                name: "IX_Cronogramas_IdDependencia",
                table: "Cronogramas",
                column: "IdDependencia");

            migrationBuilder.CreateIndex(
                name: "IX_SublineasInvestigacion_IdLinea",
                table: "SublineasInvestigacion",
                column: "IdLinea");

            migrationBuilder.AddForeignKey(
                name: "FK_Cronogramas_Cronogramas_IdDependencia",
                table: "Cronogramas",
                column: "IdDependencia",
                principalTable: "Cronogramas",
                principalColumn: "IdCronograma",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proyectos_SublineasInvestigacion_IdSublinea",
                table: "Proyectos",
                column: "IdSublinea",
                principalTable: "SublineasInvestigacion",
                principalColumn: "IdSublinea",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cronogramas_Cronogramas_IdDependencia",
                table: "Cronogramas");

            migrationBuilder.DropForeignKey(
                name: "FK_Proyectos_SublineasInvestigacion_IdSublinea",
                table: "Proyectos");

            migrationBuilder.DropTable(
                name: "SublineasInvestigacion");

            migrationBuilder.DropIndex(
                name: "IX_Proyectos_IdSublinea",
                table: "Proyectos");

            migrationBuilder.DropIndex(
                name: "IX_Cronogramas_IdDependencia",
                table: "Cronogramas");

            migrationBuilder.DropColumn(
                name: "IdSublinea",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "EsHito",
                table: "Cronogramas");

            migrationBuilder.DropColumn(
                name: "IdDependencia",
                table: "Cronogramas");

            migrationBuilder.DropColumn(
                name: "PorcentajeAvance",
                table: "Cronogramas");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Cronogramas");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "Cronogramas");

            migrationBuilder.DropColumn(
                name: "Riesgo",
                table: "Cronogramas");
        }
    }
}
