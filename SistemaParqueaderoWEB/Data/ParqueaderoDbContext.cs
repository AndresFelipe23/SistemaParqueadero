using Microsoft.EntityFrameworkCore;
using SistemaParqueaderoWEB.Models;

namespace SistemaParqueaderoWEB.Data
{
    public class ParqueaderoDbContext : DbContext
    {
        public ParqueaderoDbContext(DbContextOptions<ParqueaderoDbContext> options)
            : base(options)
        {
        }

        // DbSets para todas las entidades
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<RegistroParqueo> RegistrosParqueo { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        public DbSet<CierreCaja> CierresCaja { get; set; }
        public DbSet<Configuracion> Configuracion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Documento).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Documento).IsUnique();
                entity.Property(e => e.UsuarioNombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.UsuarioNombre).IsUnique();
                entity.Property(e => e.Contrasena).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Rol).IsRequired().HasMaxLength(50);
            });

            // Configuración de Vehiculo
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Placa).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.Placa).IsUnique();
                entity.Property(e => e.TipoVehiculo).IsRequired();
                entity.Property(e => e.Marca).HasMaxLength(50);
                entity.Property(e => e.Modelo).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(30);
            });

            // Configuración de Tarifa
            modelBuilder.Entity<Tarifa>(entity =>
            {
                entity.ToTable("Tarifas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoVehiculo).IsRequired();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TipoCobro).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DescuentoPorcentaje).HasColumnType("decimal(5,2)");
                entity.HasOne(e => e.UsuarioCreacion)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de RegistroParqueo
            modelBuilder.Entity<RegistroParqueo>(entity =>
            {
                entity.ToTable("RegistrosParqueo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VehiculoId).IsRequired();
                entity.Property(e => e.CodigoBarras).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.CodigoBarras).IsUnique();
                entity.Property(e => e.FechaEntrada).IsRequired();
                entity.Property(e => e.MontoTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DescuentoAplicado).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoFinal).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Vehiculo)
                    .WithMany(v => v.RegistrosParqueo)
                    .HasForeignKey(e => e.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.UsuarioEntrada)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioEntradaId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.UsuarioSalida)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioSalidaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pagos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RegistroParqueoId).IsRequired();
                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MetodoPago).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EstadoPago).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.RegistroParqueo)
                    .WithMany(r => r.Pagos)
                    .HasForeignKey(e => e.RegistroParqueoId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.UsuarioPago)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioPagoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de MovimientoCaja
            modelBuilder.Entity<MovimientoCaja>(entity =>
            {
                entity.ToTable("MovimientosCaja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoMovimiento).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Concepto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Pago)
                    .WithMany(p => p.MovimientosCaja)
                    .HasForeignKey(e => e.PagoId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de CierreCaja
            modelBuilder.Entity<CierreCaja>(entity =>
            {
                entity.ToTable("CierresCaja");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MontoInicial).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoEfectivo).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoTarjeta).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoTransferencia).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoEsperado).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Diferencia).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Configuracion
            modelBuilder.Entity<Configuracion>(entity =>
            {
                entity.ToTable("Configuracion");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Clave).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Clave).IsUnique();
                entity.Property(e => e.Valor).IsRequired().HasMaxLength(500);
                entity.HasOne(e => e.UsuarioActualizacion)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioActualizacionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
