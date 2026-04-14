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
        public DbSet<SublineaInvestigacion> SublineasInvestigacion => Set<SublineaInvestigacion>();
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
        public DbSet<UsuarioSistema> UsuariosSistema => Set<UsuarioSistema>();
        public DbSet<RolSistema> RolesSistema => Set<RolSistema>();
        public DbSet<TareaInvestigacion> TareasInvestigacion => Set<TareaInvestigacion>();
        public DbSet<BitacoraProyecto> BitacoraProyecto => Set<BitacoraProyecto>();
        public DbSet<SeguimientoMuestra> SeguimientosMuestra => Set<SeguimientoMuestra>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RolSistema>()
                .HasIndex(r => r.Nombre)
                .IsUnique();

            modelBuilder.Entity<UsuarioSistema>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UsuarioSistema>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<UsuarioSistema>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UsuarioSistema>()
                .HasOne(u => u.Investigador)
                .WithMany()
                .HasForeignKey(u => u.IdInvestigador)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TareaInvestigacion>()
                .HasOne(t => t.Proyecto)
                .WithMany(p => p.Tareas)
                .HasForeignKey(t => t.IdProyecto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TareaInvestigacion>()
                .HasOne(t => t.Experimento)
                .WithMany(e => e.Tareas)
                .HasForeignKey(t => t.IdExperimento)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TareaInvestigacion>()
                .HasOne(t => t.Responsable)
                .WithMany()
                .HasForeignKey(t => t.IdResponsable)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BitacoraProyecto>()
                .HasOne(b => b.Proyecto)
                .WithMany(p => p.Bitacora)
                .HasForeignKey(b => b.IdProyecto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BitacoraProyecto>()
                .HasOne(b => b.Usuario)
                .WithMany()
                .HasForeignKey(b => b.IdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SeguimientoMuestra>()
                .HasOne(s => s.Muestra)
                .WithMany(m => m.Seguimientos)
                .HasForeignKey(s => s.IdMuestra)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeguimientoMuestra>()
                .HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.IdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.SublineaInvestigacion)
                .WithMany(s => s.Proyectos)
                .HasForeignKey(p => p.IdSublinea)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SublineaInvestigacion>()
                .HasOne(s => s.LineaInvestigacion)
                .WithMany(l => l.Sublineas)
                .HasForeignKey(s => s.IdLinea)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Cronograma>()
                .HasOne(c => c.Dependencia)
                .WithMany(c => c.Dependientes)
                .HasForeignKey(c => c.IdDependencia)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
