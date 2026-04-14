namespace ResearchHub.Models
{
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Investigador = "Investigador";
        public const string InvestigadorPrincipal = "Investigador Principal";
        public const string RevisorCientifico = "Revisor Cientifico";
        public const string TecnicoLaboratorio = "Tecnico de Laboratorio";
        public const string AnalistaDatos = "Analista de Datos";
        public const string ColaboradorExterno = "Colaborador Externo";

        public const string PuedeProponerProyecto = Administrador + "," + Investigador + "," + InvestigadorPrincipal;

        public static readonly string[] RolesRegistroPublico =
        {
            Investigador,
            TecnicoLaboratorio,
            AnalistaDatos,
            ColaboradorExterno
        };

        public static readonly string[] RolesOperativos =
        {
            Investigador,
            InvestigadorPrincipal,
            RevisorCientifico,
            TecnicoLaboratorio,
            AnalistaDatos,
            ColaboradorExterno
        };
    }
}
