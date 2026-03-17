using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Models;

namespace ResearchHub.Data
{
    public class ResearchHubContext : IdentityDbContext<ApplicationUser>
    {
        public ResearchHubContext(DbContextOptions<ResearchHubContext> options) : base(options)
        {
        }

        public DbSet<Investigador> Investigadores => Set<Investigador>();
        public DbSet<Proyecto> Proyectos => Set<Proyecto>();
        public DbSet<LineaInvestigacion> LineasInvestigacion => Set<LineaInvestigacion>();
        public DbSet<Experimento> Experimentos => Set<Experimento>();
        public DbSet<Muestra> Muestras => Set<Muestra>();
        public DbSet<Laboratorio> Laboratorios => Set<Laboratorio>();
        public DbSet<EquipoLaboratorio> EquiposLaboratorio => Set<EquipoLaboratorio>();
        public DbSet<Protocolo> Protocolos => Set<Protocolo>();
        public DbSet<Variable> Variables => Set<Variable>();
        public DbSet<Resultado> Resultados => Set<Resultado>();
        public DbSet<Analisis> Analisis => Set<Analisis>();
        public DbSet<Publicacion> Publicaciones => Set<Publicacion>();
        public DbSet<RepositorioDatos> RepositoriosDatos => Set<RepositorioDatos>();
        public DbSet<Institucion> Instituciones => Set<Institucion>();
        public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
        public DbSet<Cronograma> Cronogramas => Set<Cronograma>();
        public DbSet<Validacion> Validaciones => Set<Validacion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.InvestigadorPrincipal)
                .WithMany(i => i.ProyectosPrincipales)
                .HasForeignKey(p => p.IdInvestigadorPrincipal)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Institucion)
                .WithMany(i => i.Proyectos)
                .HasForeignKey(p => p.IdInstitucion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.LineaInvestigacion)
                .WithMany(l => l.Proyectos)
                .HasForeignKey(p => p.IdLinea)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Investigador>()
                .HasOne(i => i.Institucion)
                .WithMany(i => i.Investigadores)
                .HasForeignKey(i => i.IdInstitucion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Experimento>()
                .HasOne(e => e.Proyecto)
                .WithMany(p => p.Experimentos)
                .HasForeignKey(e => e.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Experimento>()
                .HasOne(e => e.Protocolo)
                .WithMany(p => p.Experimentos)
                .HasForeignKey(e => e.IdProtocolo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Experimento>()
                .HasOne(e => e.Laboratorio)
                .WithMany(l => l.Experimentos)
                .HasForeignKey(e => e.IdLaboratorio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Muestra>()
                .HasOne(m => m.Proyecto)
                .WithMany(p => p.Muestras)
                .HasForeignKey(m => m.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EquipoLaboratorio>()
                .HasOne(e => e.Laboratorio)
                .WithMany(l => l.Equipos)
                .HasForeignKey(e => e.IdLaboratorio)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Resultado>()
                .HasOne(r => r.Experimento)
                .WithMany(e => e.Resultados)
                .HasForeignKey(r => r.IdExperimento)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Resultado>()
                .HasOne(r => r.Variable)
                .WithMany(v => v.Resultados)
                .HasForeignKey(r => r.IdVariable)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Analisis>()
                .HasOne(a => a.Resultado)
                .WithMany(r => r.Analisis)
                .HasForeignKey(a => a.IdResultado)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Validacion>()
                .HasOne(v => v.Analisis)
                .WithMany(a => a.Validaciones)
                .HasForeignKey(v => v.IdAnalisis)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Publicacion>()
                .HasOne(p => p.Proyecto)
                .WithMany(pj => pj.Publicaciones)
                .HasForeignKey(p => p.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepositorioDatos>()
                .HasOne(r => r.Proyecto)
                .WithMany(p => p.Repositorios)
                .HasForeignKey(r => r.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Proyecto)
                .WithMany(p => p.Colaboradores)
                .HasForeignKey(c => c.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cronograma>()
                .HasOne(c => c.Proyecto)
                .WithMany(p => p.Cronogramas)
                .HasForeignKey(c => c.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
