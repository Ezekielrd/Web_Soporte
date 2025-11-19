using DGASoporte.Models;
using Microsoft.EntityFrameworkCore;

namespace DGASoporte.Data
{
    public class DGADbContext : DbContext
    {
        public DGADbContext(DbContextOptions<DGADbContext> options) : base(options) { }

        // DbSets
        public DbSet<Tarea> Tareas { get; set; } = default!;
        public DbSet<Unidad> Unidades { get; set; } = default!;
        public DbSet<Categoria> Categorias { get; set; } = default!;
        public DbSet<Rol> Roles { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Tecnico> Tecnicos { get; set; } = default!;
        public DbSet<Nivel> Niveles { get; set; } = default!;
        public DbSet<Comentario> Comentarios { get; set; } = default!;
        public DbSet<Solicitud> Solicitudes { get; set; } = default!;
        public DbSet<TipoIncidencia> TipoIncidencias { get; set; } = default!;
        public DbSet<TipoServicio> TipoServicios { get; set; } = default!;
        public DbSet<Asignacion> Asignaciones { get; set; } = default!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tablas
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Tecnico>().ToTable("Tecnico");
            modelBuilder.Entity<Tarea>().ToTable("Tarea");
            modelBuilder.Entity<Unidad>().ToTable("Unidad");
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Nivel>().ToTable("Nivel");
            modelBuilder.Entity<Comentario>().ToTable("Comentario");


            // Usuario base
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("Usuario");
                e.HasKey(u => u.Id);
                e.Property(u => u.Id)
                 .HasColumnName("Id")
                 .ValueGeneratedOnAdd();
                e.Property(u => u.Usher)
                 .HasMaxLength(50)
                 .IsRequired();
                e.Property(u => u.Email)
                 .HasMaxLength(150)
                 .IsRequired();
                e.Property(u => u.NombreCompleto)
                 .HasMaxLength(150)
                 .IsRequired();
                e.Property(u => u.Codigo)
                 .HasMaxLength(30)
                 .IsRequired();
                e.Property(u => u.PasswordHash)
                 .HasMaxLength(256);
                e.Property(u => u.PasswordSalt)
                 .HasMaxLength(128);
                // Índices únicos
                e.HasIndex(u => u.Email).IsUnique();                   
                e.HasIndex(u => u.Usher).IsUnique();                    
                e.HasIndex(u => u.Codigo).IsUnique();                  

                // Relación con Rol
                e.HasOne(u => u.Rol)
                 .WithMany(r => r.Usuarios)
                 .HasForeignKey(u => u.RolId)
                 .OnDelete(DeleteBehavior.Restrict); 

                e.Property(u => u.RowVersion).IsRowVersion();
            });

            // Mapeo de Tecnico (1–1 por clave compartida)
            modelBuilder.Entity<Tecnico>(b =>
            {
                b.ToTable("Tecnico");

                b.HasKey(t => t.Id);

                b.Property(t => t.NivelId).IsRequired();

                b.HasOne(t => t.Usuario)
                 .WithOne(u => u.Tecnico)
                 .HasForeignKey<Tecnico>(t => t.Id)
                 .OnDelete(DeleteBehavior.Restrict); 
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
                e.Property(t => t.FechaLimite).HasColumnType("datetime").IsRequired(false);

                e.HasOne(t => t.Tecnico)
                 .WithMany(te => te.TareasAsignadas) // inversa en Tecnico
                 .HasForeignKey(t => t.TecnicoId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tarea_TecnicoAsignado");

                e.HasOne(t => t.Categoria)
                 .WithMany(c => c.Tareas)   // inversa en Categoria
                 .HasForeignKey(t => t.CategoriaId)
                 .IsRequired(true)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tareas_Categoria");

                e.HasOne(t => t.Unidad)
                 .WithMany(u => u.Tareas)   // inversa en Unidad
                 .HasForeignKey(t => t.UnidadId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Tarea_Unidad");
            });
        }
    }
}
