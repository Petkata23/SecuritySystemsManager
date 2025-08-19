using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Enums;

namespace SecuritySystemsManager.Data
{
    public static class DataSeeder
    {
        // Статична дата за seed данните
        private static readonly DateTime _seedDate = new DateTime(2023, 1, 1);

        public static void SeedData(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);
            SeedLocations(modelBuilder);
            SeedOrders(modelBuilder);
            SeedInstalledDevices(modelBuilder);
            SeedMaintenanceLogs(modelBuilder);
            SeedMaintenanceDevices(modelBuilder);
            SeedInvoices(modelBuilder);
            SeedNotifications(modelBuilder);
            SeedChatMessages(modelBuilder);
            SeedDropboxTokens(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
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
        }

        private static void SeedUsers(ModelBuilder modelBuilder)
        {
            var passwordHasher = new PasswordHasher<User>();

            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@securitysystems.com",
                    NormalizedEmail = "ADMIN@SECURITYSYSTEMS.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+359888123456",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = (int)RoleType.Admin,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new User
                {
                    Id = 2,
                    UserName = "john.doe",
                    NormalizedUserName = "JOHN.DOE",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    NormalizedEmail = "JOHN.DOE@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+359888111111",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = (int)RoleType.Client,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new User
                {
                    Id = 3,
                    UserName = "jane.smith",
                    NormalizedUserName = "JANE.SMITH",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    NormalizedEmail = "JANE.SMITH@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+359888222222",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = (int)RoleType.Client,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new User
                {
                    Id = 4,
                    UserName = "tech.mike",
                    NormalizedUserName = "TECH.MIKE",
                    FirstName = "Mike",
                    LastName = "Johnson",
                    Email = "mike.johnson@securitysystems.com",
                    NormalizedEmail = "MIKE.JOHNSON@SECURITYSYSTEMS.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+359888333333",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = (int)RoleType.Technician,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new User
                {
                    Id = 5,
                    UserName = "tech.sarah",
                    NormalizedUserName = "TECH.SARAH",
                    FirstName = "Sarah",
                    LastName = "Wilson",
                    Email = "sarah.wilson@securitysystems.com",
                    NormalizedEmail = "SARAH.WILSON@SECURITYSYSTEMS.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+359888444444",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = (int)RoleType.Technician,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            };

            // Хеширане на паролите
            foreach (var user in users)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, "string");
            }

            modelBuilder.Entity<User>().HasData(users);
        }

        private static void SeedUserRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { RoleId = (int)RoleType.Admin, UserId = 1 },
                new IdentityUserRole<int> { RoleId = (int)RoleType.Client, UserId = 2 },
                new IdentityUserRole<int> { RoleId = (int)RoleType.Client, UserId = 3 },
                new IdentityUserRole<int> { RoleId = (int)RoleType.Technician, UserId = 4 },
                new IdentityUserRole<int> { RoleId = (int)RoleType.Technician, UserId = 5 }
            );
        }

        private static void SeedLocations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>().HasData(
                new Location
                {
                    Id = 1,
                    Name = "Downtown Office Building",
                    Address = "123 Main Street, Sofia, Bulgaria",
                    Latitude = "42.6977",
                    Longitude = "23.3219",
                    Description = "Modern office building in the heart of Sofia",
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new Location
                {
                    Id = 2,
                    Name = "Residential Complex",
                    Address = "456 Park Avenue, Sofia, Bulgaria",
                    Latitude = "42.6500",
                    Longitude = "23.3500",
                    Description = "Luxury residential complex with 24/7 security",
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new Location
                {
                    Id = 3,
                    Name = "Shopping Mall",
                    Address = "789 Business District, Sofia, Bulgaria",
                    Latitude = "42.6800",
                    Longitude = "23.3200",
                    Description = "Large shopping mall with multiple entrances",
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedOrders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SecuritySystemOrder>().HasData(
                new SecuritySystemOrder
                {
                    Id = 1,
                    Title = "Complete Office Security System",
                    Description = "Installation of comprehensive security system including CCTV cameras, access control, and alarm system for the downtown office building.",
                    PhoneNumber = "+359888111111",
                    ClientId = 2,
                    LocationId = 1,
                    Status = OrderStatus.InProgress,
                    RequestedDate = _seedDate.AddDays(10),
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new SecuritySystemOrder
                {
                    Id = 2,
                    Title = "Residential Security Upgrade",
                    Description = "Upgrade existing security system with modern smart locks and motion sensors for the residential complex.",
                    PhoneNumber = "+359888222222",
                    ClientId = 3,
                    LocationId = 2,
                    Status = OrderStatus.Completed,
                    RequestedDate = _seedDate.AddDays(5),
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new SecuritySystemOrder
                {
                    Id = 3,
                    Title = "Mall Surveillance System",
                    Description = "Installation of high-definition surveillance cameras throughout the shopping mall for enhanced security monitoring.",
                    PhoneNumber = "+359888111111",
                    ClientId = 2,
                    LocationId = 3,
                    Status = OrderStatus.Pending,
                    RequestedDate = _seedDate.AddDays(15),
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedInstalledDevices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InstalledDevice>().HasData(
                new InstalledDevice
                {
                    Id = 1,
                    SecuritySystemOrderId = 1,
                    DeviceType = DeviceType.Camera,
                    Brand = "Hikvision",
                    Model = "DS-2CD2142FWD-I",
                    Quantity = 8,
                    DateInstalled = _seedDate.AddDays(12),
                    InstalledById = 4,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new InstalledDevice
                {
                    Id = 2,
                    SecuritySystemOrderId = 1,
                    DeviceType = DeviceType.AccessControl,
                    Brand = "Honeywell",
                    Model = "PRO3000",
                    Quantity = 4,
                    DateInstalled = _seedDate.AddDays(12),
                    InstalledById = 4,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new InstalledDevice
                {
                    Id = 3,
                    SecuritySystemOrderId = 2,
                    DeviceType = DeviceType.Alarm,
                    Brand = "Bosch",
                    Model = "Solution 16 Plus",
                    Quantity = 1,
                    DateInstalled = _seedDate.AddDays(7),
                    InstalledById = 5,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new InstalledDevice
                {
                    Id = 4,
                    SecuritySystemOrderId = 2,
                    DeviceType = DeviceType.AccessControl,
                    Brand = "Yale",
                    Model = "YRD256",
                    Quantity = 12,
                    DateInstalled = _seedDate.AddDays(7),
                    InstalledById = 5,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedMaintenanceLogs(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MaintenanceLog>().HasData(
                new MaintenanceLog
                {
                    Id = 1,
                    SecuritySystemOrderId = 1,
                    TechnicianId = 4,
                    Date = _seedDate.AddDays(20),
                    Description = "Routine maintenance check of CCTV cameras. All cameras functioning properly. Cleaned camera lenses and checked recording system.",
                    Resolved = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new MaintenanceLog
                {
                    Id = 2,
                    SecuritySystemOrderId = 2,
                    TechnicianId = 5,
                    Date = _seedDate.AddDays(25),
                    Description = "Smart lock battery replacement and firmware update. All locks now have latest security patches.",
                    Resolved = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedMaintenanceDevices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MaintenanceDevice>().HasData(
                new MaintenanceDevice
                {
                    Id = 1,
                    MaintenanceLogId = 1,
                    InstalledDeviceId = 1,
                    Notes = "CCTV cameras cleaned and tested",
                    IsFixed = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new MaintenanceDevice
                {
                    Id = 2,
                    MaintenanceLogId = 2,
                    InstalledDeviceId = 4,
                    Notes = "Smart locks updated and batteries replaced",
                    IsFixed = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedInvoices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    Id = 1,
                    SecuritySystemOrderId = 1,
                    IssuedOn = _seedDate.AddDays(15),
                    TotalAmount = 12500.00m,
                    IsPaid = false,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new Invoice
                {
                    Id = 2,
                    SecuritySystemOrderId = 2,
                    IssuedOn = _seedDate.AddDays(10),
                    TotalAmount = 8500.00m,
                    IsPaid = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedNotifications(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>().HasData(
                new Notification
                {
                    Id = 1,
                    RecipientId = 2,
                    Message = "Your security system installation has been completed successfully.",
                    DateSent = _seedDate.AddDays(12),
                    IsRead = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new Notification
                {
                    Id = 2,
                    RecipientId = 3,
                    Message = "Maintenance scheduled for your security system on next Monday.",
                    DateSent = _seedDate.AddDays(22),
                    IsRead = false,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new Notification
                {
                    Id = 3,
                    RecipientId = 4,
                    Message = "New order assigned: Mall Surveillance System installation.",
                    DateSent = _seedDate.AddDays(1),
                    IsRead = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedChatMessages(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>().HasData(
                new ChatMessage
                {
                    Id = 1,
                    Message = "Hello! I have a question about my security system installation.",
                    SenderId = 2,
                    SenderName = "John Doe",
                    RecipientId = 4,
                    RecipientName = "Mike Johnson",
                    IsRead = true,
                    ReadAt = _seedDate.AddDays(1),
                    Timestamp = _seedDate.AddDays(1),
                    IsFromSupport = false,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new ChatMessage
                {
                    Id = 2,
                    Message = "Hi John! I'd be happy to help you with any questions about your security system.",
                    SenderId = 4,
                    SenderName = "Mike Johnson",
                    RecipientId = 2,
                    RecipientName = "John Doe",
                    IsRead = true,
                    ReadAt = _seedDate.AddDays(1),
                    Timestamp = _seedDate.AddDays(1).AddMinutes(5),
                    IsFromSupport = true,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                },
                new ChatMessage
                {
                    Id = 3,
                    Message = "When will the installation be completed?",
                    SenderId = 2,
                    SenderName = "John Doe",
                    RecipientId = 4,
                    RecipientName = "Mike Johnson",
                    IsRead = false,
                    Timestamp = _seedDate.AddDays(1).AddMinutes(10),
                    IsFromSupport = false,
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }

        private static void SeedDropboxTokens(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DropboxToken>().HasData(
                new DropboxToken
                {
                    Id = 1,
                    AccessToken = "sl.u.AF483kOBybKknUScHzAGW89mmIAzptz3uaXYHFlHGPuHg_arYJjeKV7AbfqwUtaGAsg8xxi35Fzr8EPKwAz1ZqlIKx_98O1msKUamXdkafMDhVvT9pOEQVq0LQZBFF5AXoHROv7nm6CzuteTmjx7fYEUVUzI7Kut0anRUd3QbogaUUpBls2pFp2ayqjyNapP1hPcDlwlzc2VVZ1HLsRfMdk3SPLU3otruK2S5XsuH9FHua9tYaI2HE2kxZtQQ4ZXJZzjqGuHoxWoCgzf6x6xhPoSF930aDuYydkkGH0B-_fLP8nFDE88_IJ5xVlHr0F86-Bmxqe-RfLW6xtmrNUlg4Hq_vTFYDEoaiXvOG91HVi5AhJ2BXjEMT2zkpzoqVP1VNK1zeky-oYn6ktGMJSvd3QJxa9MyT_n0uxM65B2fvGTyYt8o6kHfSOCoLhBgZRJn5TcWjy5k4lK8pZbItmc_L1wJn-HGE0xUhXne2wOT6m4yz2ctorke2TevrbUWszls1Pj0xgjvkgxps9wOiqrd3mt0ItnmI0KBQyXyaJ2OhgrReDYiMcOHp9L8fpjbjITpXvhF4yThqPnU63xumQQhgdGG3U2xxTnuopHhsuQbOXai1P7LOWaDtfhAsKWqwV8UZkRQhYIvzrUIszos8fCr7ZC7EWAXqnIfaWnYs-eMmJVDpJHTmRhQvL4cqYy7C5-TrhdXBf2iyWiJ5-EZUJXsKr-1qOGoCwexsU91VtYdFtC0y_dcl3KtQ4LeZTpiyqEif97QB0mUYfSWYVAO4pCU_ScnbG5IdnwBwpF6o4xwIOenRpai-gd88UVmlgSq200ZlxcUu__dzuk5TVhuqsc21m4jAVVE9qweCiud0xT0qAJBvuS_Ztq3Vxhvm63v9JjLh1mOfrEzksd-S2qWnb7DJjnN6K11IwSPgTf7GRItFjItgeOSsnHi_RSyYgaKW_s0_Tn2T9WQL3CRLUGdKGO2efiFRhNVw9OPr1Givx5eFGmJOWt62HfM_ZnaM1XhBnwfT5EaLEWl8aMC0QaATvtmVdj-Q7Aq2m8QDsy0cMQCzIbu8yp7f2S2ndXW4nFOx4On-CBwGB2I1jKJPEnmLaLoDlhrnRwdgK07PGubvupN1M7N-inKa9LSvOSXnCFhjI_C-631rPck5QxKI1b5oKwlHVD9KopEhQYlcdNd7MHaTEsNrktl3wEP2xUAUycXAqJXY2AAW6oC89aCgnC_9ab9eSAiERnpk5sZtwoTshU_MpAtiGg12vGv7YftBvnhXEqNEaZhcD1r_j-ZWUQnKPaYBdi",
                    RefreshToken = "moaIvwyc8-UAAAAAAAAAAc6ZNtmMh1S0twz1gOImaWzk_b2p76GFqV-mB1Nm9U1s",
                    ExpiryTime = new DateTime(2025, 8, 1, 2, 28, 28),
                    CreatedAt = _seedDate,
                    UpdatedAt = _seedDate
                }
            );
        }
    }
}