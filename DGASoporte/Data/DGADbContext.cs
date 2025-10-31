using DGASoporte.Models;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Data
{
    public class DGADbContext : DbContext
    {
        public DGADbContext(DbContextOptions<DGADbContext> options) : base(options) { }

        // DbSets
        public DbSet<Tarea> Tareas { get; set; } = default!;
        public DbSet<Estado> Estados { get; set; } = default!;
        public DbSet<Unidad> Unidades { get; set; } = default!;
        public DbSet<Prioridad> Prioridades { get; set; } = default!;
        public DbSet<Categoria> Categorias { get; set; } = default!;
        public DbSet<Division> Divisiones { get; set; } = default!;
        public DbSet<Rol> Roles { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Tecnico> Tecnicos { get; set; } = default!;
        public DbSet<Nivel> Nivles { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tablas
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Tecnico>().ToTable("Tecnico");
            modelBuilder.Entity<Tarea>().ToTable("Tarea");
            modelBuilder.Entity<Estado>().ToTable("Estado");
            modelBuilder.Entity<Unidad>().ToTable("Unidad");
            modelBuilder.Entity<Prioridad>().ToTable("Prioridad");
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Division>().ToTable("Division");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Nivel>().ToTable("Nivel");

            // Usuario base
            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Id)
                 .HasColumnName("Id")
                 .ValueGeneratedOnAdd();

                e.HasIndex(u => u.Email).IsUnique();

                e.HasOne(u => u.Rol)
                 .WithMany(r => r.Usuarios)
                 .HasForeignKey(u => u.RolId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Tecnico: herencia TPT de Usuario
            modelBuilder.Entity<Tecnico>(e =>
            {
                e.HasBaseType<Usuario>();     // <- importante para TPT
                // No configures HasOne/WithOne ni ValueGeneratedNever aquí.
            });

            // Rol
            modelBuilder.Entity<Rol>(e =>
            {
                e.HasKey(r => r.Id);
                e.HasIndex(r => r.Nombre).IsUnique();
            });
            // Tarea + inversas explícitas (opción A)
            modelBuilder.Entity<Tarea>(e =>
            {
                e.HasKey(t => t.Id);

                e.Property(t => t.FechaCreacion).HasColumnType("datetime");
                e.Property(t => t.FechaLimite).HasColumnType("datetime");

                e.HasOne(t => t.Tecnico)
                 .WithMany(te => te.Tareas) // inversa en Tecnico
                 .HasForeignKey(t => t.TecnicoId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tarea_TecnicoAsignado");

                e.HasOne(t => t.Categoria)
                 .WithMany(c => c.Tareas)   // inversa en Categoria
                 .HasForeignKey(t => t.CategoriaId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tareas_Categoria");

                e.HasOne(t => t.Prioridad)
                 .WithMany(p => p.Tareas)   // inversa en Prioridad
                 .HasForeignKey(t => t.PrioridadId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tareas_Prioridad");

                e.HasOne(t => t.Estado)
                 .WithMany(es => es.Tareas) // inversa en Estado
                 .HasForeignKey(t => t.EstadoId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tareas_Estado");

                e.HasOne(t => t.Unidad)
                 .WithMany(u => u.Tareas)   // inversa en Unidad
                 .HasForeignKey(t => t.UnidadId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tarea_Unidad");
            });
        }
    }
}
