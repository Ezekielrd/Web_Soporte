using DGASoporte.Models;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Data
{
    public class DGADbContext : DbContext
    {
        public DGADbContext(DbContextOptions<DGADbContext> options) : base(options) { }

        // DbSets
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Unidad> Unidades { get; set; }
        public DbSet<Prioridad> Prioridades { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Division> Divisiones { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Tecnico> Tecnicos { get; set; }

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

            // Usuario (base): IDENTITY
            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(u => u.Id);

                // Asegura store-generated en BD:
                e.Property(u => u.Id)
                 .HasColumnName("Id")
                 .ValueGeneratedOnAdd(); // <- usa esto (equivale a identity); si quieres, añade .UseIdentityColumn()

                e.HasIndex(u => u.Email).IsUnique();

                e.HasOne(u => u.Rol)
                 .WithMany(r => r.Usuarios)
                 .HasForeignKey(u => u.RolId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Tecnico (TPT): NO generado, usa el Id del base
            modelBuilder.Entity<Tecnico>(e =>
            {
                e.Property(t => t.Id)
                 .HasColumnName("Id")
                 .ValueGeneratedNever(); // <- importante

                e.HasOne<Usuario>()
                 .WithOne()
                 .HasForeignKey<Tecnico>(t => t.Id)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasConstraintName("FK_Tecnico_Usuario");
            });

            // Rol
            modelBuilder.Entity<Rol>(e =>
            {
                e.HasKey(r => r.Id);
                e.HasIndex(r => r.Nombre).IsUnique();
            });

            // Tarea
            modelBuilder.Entity<Tarea>(e =>
            {
                e.HasKey(t => t.Id);

                e.Property(t => t.FechaCreacion).HasColumnType("datetime");
                e.Property(t => t.FechaLimite).HasColumnType("datetime");

                e.HasOne(t => t.Tecnico)
                    .WithMany(te => te.Tareas)
                    .HasForeignKey(t => t.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tarea_TecnicoAsignado");

                e.HasOne(t => t.Categoria)
                    .WithMany()
                    .HasForeignKey(t => t.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tareas_Categoria");

                e.HasOne(t => t.Prioridad)
                    .WithMany()
                    .HasForeignKey(t => t.PrioridadId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tareas_Prioridad");

                e.HasOne(t => t.Estado)
                    .WithMany()
                    .HasForeignKey(t => t.EstadoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tareas_Estado");

                e.HasOne(t => t.Unidad)
                    .WithMany()
                    .HasForeignKey(t => t.UnidadId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Tarea_Unidad");
            });
        }
    }
}
