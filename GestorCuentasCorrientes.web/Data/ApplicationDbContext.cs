using GestorCuentasCorrientes.web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestorCuentasCorrientes.web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        // DbSets para todas las entidades del dominio
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; }
        public DbSet<MedioPago> MediosPago { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Cheque> Cheques { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }

        //fluent API configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== LOCALIDAD =====
            modelBuilder.Entity<Localidad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Provincia).HasMaxLength(100);
            });

            // ===== TIPO MOVIMIENTO =====
            modelBuilder.Entity<TipoMovimiento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Signo).IsRequired();
                // CHECK constraint: Signo IN (1, -1)
                entity.HasCheckConstraint("CK_TiposMovimiento_Signo", "Signo IN (1, -1)");
            });

            // ===== MEDIO PAGO =====
            modelBuilder.Entity<MedioPago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // ===== USUARIO =====
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            });

            // ===== CLIENTE =====
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CuitDni).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Telefono).HasMaxLength(30);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaAlta).HasDefaultValueSql("GETDATE()");

                // Índice único filtrado para CuitDni (permite múltiples NULL)
                entity.HasIndex(e => e.CuitDni)
                    .IsUnique()
                    .HasFilter("[CuitDni] IS NOT NULL");

                // Relación con Localidad (N:1)
                entity.HasOne(e => e.Localidad)
                    .WithMany(l => l.Clientes)
                    .HasForeignKey(e => e.LocalidadId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Movimientos (1:N)
                entity.HasMany(e => e.Movimientos)
                    .WithOne(m => m.Cliente)
                    .HasForeignKey(m => m.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== MOVIMIENTO =====
            // CRÍTICO: Movimiento tiene 4 relaciones (Cliente, TipoMovimiento, Usuario, MovimientoOrigen)
            // TODAS con DeleteBehavior.Restrict para evitar errores de múltiples rutas de cascada
            modelBuilder.Entity<Movimiento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Fecha).IsRequired();
                entity.Property(e => e.NumeroComprobante).HasMaxLength(30);
                entity.Property(e => e.Importe).IsRequired().HasPrecision(12, 2);
                entity.Property(e => e.UsuarioId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Anulado).HasDefaultValue(false);

                // CHECK constraint: Importe > 0
                entity.HasCheckConstraint("CK_Movimientos_Importe", "Importe > 0");

                // Relación con Cliente (N:1)
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Movimientos)
                    .HasForeignKey(e => e.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con TipoMovimiento (N:1)
                entity.HasOne(e => e.TipoMovimiento)
                    .WithMany(tm => tm.Movimientos)
                    .HasForeignKey(e => e.TipoMovimientoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Usuario (N:1)
                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Movimientos)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
                //Esto me gustaria entenderlo mejor y que el chat me lo explique.
                // Auto-referencia: Movimiento puede tener un MovimientoOrigen (N:1)
                // Este movimiento origina otros movimientos (1:N)
                entity.HasOne(e => e.MovimientoOrigen)
                    .WithMany(m => m.MovimientosOriginados)
                    .HasForeignKey(e => e.MovimientoOrigenId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Pagos (1:N)
                entity.HasMany(e => e.Pagos)
                    .WithOne(p => p.Movimiento)
                    .HasForeignKey(p => p.MovimientoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Comprobantes (1:N)
                entity.HasMany(e => e.Comprobantes)
                    .WithOne(c => c.Movimiento)
                    .HasForeignKey(c => c.MovimientoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== PAGO =====
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Importe).IsRequired().HasPrecision(12, 2);

                // CHECK constraint: Importe > 0
                entity.HasCheckConstraint("CK_Pagos_Importe", "Importe > 0");

                // Relación con Movimiento (N:1)
                entity.HasOne(e => e.Movimiento)
                    .WithMany(m => m.Pagos)
                    .HasForeignKey(e => e.MovimientoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con MedioPago (N:1)
                entity.HasOne(e => e.MedioPago)
                    .WithMany(mp => mp.Pagos)
                    .HasForeignKey(e => e.MedioPagoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Cheque (1:1 opcional)
                entity.HasOne(e => e.Cheque)
                    .WithOne(ch => ch.Pago)
                    .HasForeignKey<Cheque>(ch => ch.PagoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CHEQUE =====
            modelBuilder.Entity<Cheque>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Banco).HasMaxLength(100);
                entity.Property(e => e.Titular).HasMaxLength(150);
                entity.Property(e => e.FechaEmision).IsRequired();
                entity.Property(e => e.FechaCobro).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("EnCartera");

                // CHECK constraint: Estado IN ('EnCartera', 'Depositado', 'Acreditado', 'Rechazado')
                entity.HasCheckConstraint("CK_Cheques_Estado",
                    "Estado IN ('EnCartera', 'Depositado', 'Acreditado', 'Rechazado')");

                // Unique constraint en PagoId (1:1)
                entity.HasIndex(e => e.PagoId).IsUnique();

                // Relación con Pago (1:1)
                entity.HasOne(e => e.Pago)
                    .WithOne(p => p.Cheque)
                    .HasForeignKey<Cheque>(e => e.PagoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== COMPROBANTE =====
            modelBuilder.Entity<Comprobante>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreArchivo).IsRequired().HasMaxLength(255);
                entity.Property(e => e.RutaArchivo).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TipoArchivo).HasMaxLength(10);
                entity.Property(e => e.FechaCarga).HasDefaultValueSql("GETDATE()");

                // Relación con Movimiento (N:1)
                entity.HasOne(e => e.Movimiento)
                    .WithMany(m => m.Comprobantes)
                    .HasForeignKey(e => e.MovimientoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
