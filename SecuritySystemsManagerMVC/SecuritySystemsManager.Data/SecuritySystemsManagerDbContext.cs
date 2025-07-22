using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Security;

namespace SecuritySystemsManager.Data
{
    public class SecuritySystemsManagerDbContext : IdentityDbContext<User, Role, int>
    {
        // Статична дата за seed данните
        private static readonly DateTime _seedDate = new DateTime(2023, 1, 1);

        public DbSet<Location> Locations { get; set; }
        public DbSet<SecuritySystemOrder> Orders { get; set; }
        public DbSet<InstalledDevice> InstalledDevices { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<MaintenanceDevice> MaintenanceDevices { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DropboxToken> DropboxTokens { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

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

            // Промяна на имената на таблиците по подразбиране за Identity
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            // Модифициране на Identity релациите, за да избегнем циклични каскадни пътища
            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // ===== КОНФИГУРАЦИЯ НА РЕЛАЦИИТЕ =====

            // 👥 Role <-> Users (1:N) - вече се управлява от Identity

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

            // 👥 Many-to-Many: Order <-> User (Technicians)
            // Конфигуриране на many-to-many връзката между поръчки и техници
            modelBuilder.Entity<SecuritySystemOrder>()
                .HasMany(o => o.Technicians)
                .WithMany(u => u.AssignedOrders)
                .UsingEntity(j => j.ToTable("OrderTechnicians"));

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

            // 💬 ChatMessage <-> User (Sender) (N:1)
            // При изтриване на потребител, неговите изпратени съобщения се запазват (Restrict)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // 💬 ChatMessage <-> User (Recipient) (N:1)
            // При изтриване на потребител, неговите получени съобщения се запазват (Restrict)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== ДОПЪЛНИТЕЛНИ КОНФИГУРАЦИИ =====

            // 💰 Invoice TotalAmount decimal precision
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            // ✅ Уникален Username - вече се управлява от Identity

            // ===== SEED ДАННИ =====

            // 🌱 Сийдване на роли
            foreach (var roleType in Enum.GetValues(typeof(RoleType)).Cast<RoleType>())
            {
                modelBuilder.Entity<Role>().HasData(new Role
                {
                    Id = (int)roleType,
                    Name = roleType.ToString(),
                    NormalizedName = roleType.ToString().ToUpper(),
                    RoleType = roleType,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                });
            }

            // 🌱 Сийдване на потребител с роля Administrator
            var adminUser = new User
            {
                Id = 1,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@securitysystems.com",
                NormalizedEmail = "ADMIN@SECURITYSYSTEMS.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                RoleId = (int)RoleType.Admin,
                CreatedAt = _seedDate,
                UpdatedAt = _seedDate
            };
            
            // Хеширане на паролата с Identity
            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "string");
            
            modelBuilder.Entity<User>().HasData(adminUser);
            
            // Добавяне на админ потребителя към админ ролята
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int>
            {
                RoleId = (int)RoleType.Admin,
                UserId = 1
            });
        }
    }
}
