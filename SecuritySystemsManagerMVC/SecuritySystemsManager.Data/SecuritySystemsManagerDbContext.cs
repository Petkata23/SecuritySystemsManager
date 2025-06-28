using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Security;

namespace SecuritySystemsManager.Data
{
    public class SecuritySystemsManagerDbContext : DbContext
    {
        // Статична дата за seed данните
        private static readonly DateTime _seedDate = new DateTime(2023, 1, 1);

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SecuritySystemOrder> Orders { get; set; }
        public DbSet<OrderTechnician> OrderTechnicians { get; set; }
        public DbSet<InstalledDevice> InstalledDevices { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<MaintenanceDevice> MaintenanceDevices { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public SecuritySystemsManagerDbContext() { }

        public SecuritySystemsManagerDbContext(DbContextOptions<SecuritySystemsManagerDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                optionsBuilder.EnableSensitiveDataLogging();
            }

            // Конфигуриране на предупрежденията
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== КОНФИГУРАЦИЯ НА РЕЛАЦИИТЕ =====

            // 👥 Role <-> Users (1:N)
            // Ролите са основни данни и не трябва да се изтриват, ако има потребители с тази роля
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 👤 User <-> Orders as Client (1:N)
            // Клиентът не трябва да може да бъде изтрит, ако има поръчки
            modelBuilder.Entity<SecuritySystemOrder>()
                .HasOne(o => o.Client)
                .WithMany(u => u.OrdersAsClient)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🌍 Location <-> Orders (1:N)
            // При изтриване на локация, свързаните поръчки се изтриват
            modelBuilder.Entity<Location>()
                .HasMany(l => l.Orders)
                .WithOne(o => o.Location)
                .HasForeignKey(o => o.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 📦 Order <-> InstalledDevices (1:N)
            // При изтриване на поръчка, инсталираните устройства се изтриват
            modelBuilder.Entity<SecuritySystemOrder>()
                .HasMany(o => o.InstalledDevices)
                .WithOne(d => d.SecuritySystemOrder)
                .HasForeignKey(d => d.SecuritySystemOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔧 InstalledDevice <-> User (InstalledBy) (N:1)
            // Техникът не трябва да може да бъде изтрит, ако е инсталирал устройства
            modelBuilder.Entity<InstalledDevice>()
                .HasOne(d => d.InstalledBy)
                .WithMany()
                .HasForeignKey(d => d.InstalledById)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔧 Order <-> MaintenanceLogs (1:N)
            // При изтриване на поръчка, свързаните записи за поддръжка се изтриват
            modelBuilder.Entity<SecuritySystemOrder>()
                .HasMany(o => o.MaintenanceLogs)
                .WithOne(m => m.SecuritySystemOrder)
                .HasForeignKey(m => m.SecuritySystemOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔧 MaintenanceLog <-> Technician (N:1)
            // Техникът не трябва да може да бъде изтрит, ако има записи за поддръжка
            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(m => m.Technician)
                .WithMany()
                .HasForeignKey(m => m.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔁 Many-to-Many: Order <-> Technician чрез OrderTechnician
            modelBuilder.Entity<OrderTechnician>()
                .HasKey(ot => new { ot.SecuritySystemOrderId, ot.TechnicianId });

            // При изтриване на поръчка, връзките с техниците се изтриват
            modelBuilder.Entity<OrderTechnician>()
                .HasOne(ot => ot.SecuritySystemOrder)
                .WithMany(o => o.Technicians)
                .HasForeignKey(ot => ot.SecuritySystemOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Техникът не трябва да може да бъде изтрит, ако е назначен към поръчки
            modelBuilder.Entity<OrderTechnician>()
                .HasOne(ot => ot.Technician)
                .WithMany(u => u.AssignedOrders)
                .HasForeignKey(ot => ot.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔧 Many-to-Many: MaintenanceLog <-> InstalledDevice чрез MaintenanceDevice
            // При изтриване на запис за поддръжка, връзките с устройствата се изтриват
            modelBuilder.Entity<MaintenanceDevice>()
                .HasOne(md => md.MaintenanceLog)
                .WithMany(ml => ml.MaintenanceDevices)
                .HasForeignKey(md => md.MaintenanceLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // При изтриване на устройство, връзките със записите за поддръжка се запазват (Restrict)
            modelBuilder.Entity<MaintenanceDevice>()
                .HasOne(md => md.InstalledDevice)
                .WithMany(id => id.MaintenanceDevices)
                .HasForeignKey(md => md.InstalledDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            // 📄 Invoice <-> Order (1:1)
            // При изтриване на поръчка, фактурата се изтрива
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.SecuritySystemOrder)
                .WithOne()
                .HasForeignKey<Invoice>(i => i.SecuritySystemOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔔 Notification <-> User (Recipient) (N:1)
            // При изтриване на потребител, неговите известия се изтриват
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== ДОПЪЛНИТЕЛНИ КОНФИГУРАЦИИ =====

            // 💰 Invoice TotalAmount decimal precision
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            // ✅ Уникален Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ===== SEED ДАННИ =====

            // 🌱 Сийдване на роли
            foreach (var role in Enum.GetValues(typeof(RoleType)).Cast<RoleType>())
            {
                modelBuilder.Entity<Role>().HasData(new Role
                {
                    Id = (int)role,
                    Name = role.ToString(),
                    RoleType = role,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                });
            }

            // 🌱 Сийдване на потребител с роля Administrator
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                FirstName = "Admin",
                LastName = "User",
                Password = PasswordHasher.HashPassword("string"),
                RoleId = (int)RoleType.Admin,
                CreatedAt = _seedDate,
                UpdatedAt = _seedDate
            });
        }
    }
}
