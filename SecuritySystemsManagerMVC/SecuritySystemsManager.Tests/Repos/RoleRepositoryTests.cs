using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Repos
{
    [TestFixture]
    public class RoleRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private RoleRepository _repository;
        private Role _testRole;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _testRole = new Role
            {
                Id = 1,
                Name = "TestRole",
                NormalizedName = "TESTROLE",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Roles.Add(_testRole);
            _context.SaveChanges();

            _repository = new RoleRepository(_context, CreateMockMapper());
        }

        [TearDown]
        public void TearDown()
        {
            _repository?.Dispose();
            _context?.Dispose();
        }

        private AutoMapper.IMapper CreateMockMapper()
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Role, RoleDto>();
                cfg.CreateMap<RoleDto, Role>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnRole()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testRole.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testRole.Id));
            Assert.That(result.Name, Is.EqualTo(_testRole.Name));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllRoles()
        {
            // Arrange
            var secondRole = new Role
            {
                Id = 2,
                Name = "SecondRole",
                NormalizedName = "SECONDROLE",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Roles.AddAsync(secondRole);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewRole_ShouldSaveToDatabase()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Name = "NewRole"
            };

            // Act
            await _repository.SaveAsync(roleDto);

            // Assert
            var savedRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "NewRole");
            Assert.That(savedRole, Is.Not.Null);
            Assert.That(savedRole.Name, Is.EqualTo("NewRole"));
        }

        [Test]
        public async Task SaveAsync_WithExistingRole_ShouldUpdateRole()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Id = _testRole.Id,
                Name = "UpdatedRole"
            };

            // Act
            await _repository.SaveAsync(roleDto);

            // Assert
            var updatedRole = await _context.Roles.FindAsync(_testRole.Id);
            Assert.That(updatedRole, Is.Not.Null);
            Assert.That(updatedRole.Name, Is.EqualTo("UpdatedRole"));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveRole()
        {
            // Act
            await _repository.DeleteAsync(_testRole.Id);

            // Assert
            var deletedRole = await _context.Roles.FindAsync(_testRole.Id);
            Assert.That(deletedRole, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.DeleteAsync(999));
        }

        [Test]
        public async Task GetWithPaginationAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            var roles = new List<Role>();
            for (int i = 2; i <= 10; i++)
            {
                roles.Add(new Role
                {
                    Id = i,
                    Name = $"Role{i}",
                    NormalizedName = $"ROLE{i}",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(5, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task GetWithPaginationAsync_WithInvalidPageSize_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetWithPaginationAsync(0, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetWithPaginationAsync_WithInvalidPageNumber_ShouldReturnResults()
        {
            // Act
            var result = await _repository.GetWithPaginationAsync(10, 0);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WithValidName_ShouldReturnRole()
        {
            // Act
            var result = await _repository.GetByNameIfExistsAsync(_testRole.Name);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testRole.Id));
            Assert.That(result.Name, Is.EqualTo(_testRole.Name));
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WithInvalidName_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByNameIfExistsAsync("NonExistentRole");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WithNullName_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByNameIfExistsAsync(null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WithEmptyName_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByNameIfExistsAsync("");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdIfExistsAsync_WithValidId_ShouldReturnRole()
        {
            // Act
            var result = await _repository.GetByIdIfExistsAsync(_testRole.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testRole.Id));
            Assert.That(result.Name, Is.EqualTo(_testRole.Name));
        }

        [Test]
        public async Task GetByIdIfExistsAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdIfExistsAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ExistsByIdAsync_WithValidId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByIdAsync(_testRole.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ExistsByIdAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByIdAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CreateAsync_WithValidRole_ShouldCreateRole()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Name = "NewRole"
            };

            // Act
            await _repository.CreateAsync(roleDto);

            // Assert
            var createdRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "NewRole");
            Assert.That(createdRole, Is.Not.Null);
            Assert.That(createdRole.Name, Is.EqualTo("NewRole"));
        }

        [Test]
        public async Task CreateAsync_WithNullRole_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.CreateAsync(null));
        }

        [Test]
        public async Task UpdateAsync_WithValidRole_ShouldUpdateRole()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Id = _testRole.Id,
                Name = "UpdatedRole"
            };

            // Act
            await _repository.UpdateAsync(roleDto);

            // Assert
            var updatedRole = await _context.Roles.FindAsync(_testRole.Id);
            Assert.That(updatedRole, Is.Not.Null);
            Assert.That(updatedRole.Name, Is.EqualTo("UpdatedRole"));
        }

        [Test]
        public async Task UpdateAsync_WithNullRole_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.UpdateAsync(null));
        }

        [Test]
        public async Task UpdateAsync_WithNonExistentRole_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Id = 999,
                Name = "NonExistentRole"
            };

            // Act & Assert - The method should handle the exception gracefully and not throw
            Assert.DoesNotThrowAsync(async () => await _repository.UpdateAsync(roleDto));
        }
    }
} 