using System.Collections.Generic;

namespace ResearchHub.Models
{
    public class EntidadRoadmapViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public string Estado { get; set; } = "Planeado";
        public string Madurez { get; set; } = "Inicial";
        public List<string> Funciones { get; set; } = new();
        public List<string> Caracteristicas { get; set; } = new();
        public List<string> PracticasReferencia { get; set; } = new();
        public List<string> BrechasActuales { get; set; } = new();
        public List<string> AccionesPrioritarias { get; set; } = new();
    }
}
