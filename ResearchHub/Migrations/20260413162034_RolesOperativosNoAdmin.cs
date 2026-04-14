using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchHub.Migrations
{
    /// <inheritdoc />
    public partial class RolesOperativosNoAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @RolUsuarioId INT = (SELECT TOP 1 IdRol FROM RolesSistema WHERE Nombre = 'Usuario');
                DECLARE @RolInvestigadorId INT = (SELECT TOP 1 IdRol FROM RolesSistema WHERE Nombre = 'Investigador');

                IF @RolUsuarioId IS NOT NULL AND @RolInvestigadorId IS NULL
                BEGIN
                    UPDATE RolesSistema
                    SET Nombre = 'Investigador',
                        Descripcion = 'Investigador con acceso operativo a proyectos, tareas, bitacora, resultados y seguimiento cientifico.'
                    WHERE IdRol = @RolUsuarioId;
                END
                ELSE IF @RolUsuarioId IS NOT NULL AND @RolInvestigadorId IS NOT NULL
                BEGIN
                    UPDATE UsuariosSistema
                    SET IdRol = @RolInvestigadorId
                    WHERE IdRol = @RolUsuarioId;

                    DELETE FROM RolesSistema
                    WHERE IdRol = @RolUsuarioId;
                END

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Administrador')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Administrador', 'Control total del sistema, catalogos y administracion de usuarios.', 1);
                END;

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Investigador')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Investigador', 'Perfil cientifico general con acceso a proyectos, tareas, bitacora, muestras, resultados y publicaciones.', 1);
                END;

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Investigador Principal')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Investigador Principal', 'Lider cientifico del proyecto. Puede proponer proyectos y coordinar el trabajo operativo.', 1);
                END;

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Revisor Cientifico')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Revisor Cientifico', 'Perfil orientado a revisar bitacora, resultados, cronogramas y evidencia cientifica.', 1);
                END;

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Tecnico de Laboratorio')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Tecnico de Laboratorio', 'Perfil operativo de ejecucion experimental y seguimiento de muestras.', 1);
                END;

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Analista de Datos')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Analista de Datos', 'Perfil enfocado en resultados, tareas analiticas, publicaciones y trazabilidad de datos.', 1);
                END;

                IF NOT EXISTS (SELECT 1 FROM RolesSistema WHERE Nombre = 'Colaborador Externo')
                BEGIN
                    INSERT INTO RolesSistema (Nombre, Descripcion, Activo)
                    VALUES ('Colaborador Externo', 'Perfil de consulta acotada para colaboradores invitados con acceso no administrativo.', 1);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @RolUsuarioId INT = (SELECT TOP 1 IdRol FROM RolesSistema WHERE Nombre = 'Usuario');
                DECLARE @RolInvestigadorId INT = (SELECT TOP 1 IdRol FROM RolesSistema WHERE Nombre = 'Investigador');

                IF @RolInvestigadorId IS NOT NULL AND @RolUsuarioId IS NULL
                BEGIN
                    UPDATE RolesSistema
                    SET Nombre = 'Usuario',
                        Descripcion = 'Rol general de usuario.'
                    WHERE IdRol = @RolInvestigadorId;
                END

                DELETE FROM RolesSistema
                WHERE Nombre IN ('Investigador Principal', 'Revisor Cientifico', 'Tecnico de Laboratorio', 'Analista de Datos', 'Colaborador Externo')
                  AND IdRol NOT IN (SELECT DISTINCT IdRol FROM UsuariosSistema);
                """);
        }
    }
}
