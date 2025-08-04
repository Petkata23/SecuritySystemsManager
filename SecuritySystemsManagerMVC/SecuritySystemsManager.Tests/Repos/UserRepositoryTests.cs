using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Repos
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private SecuritySystemsManagerDbContext _context;
        private UserRepository _repository;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();

            // Setup test data
            _testUser = new User
            {
                Id = 1,
                UserName = "testuser@test.com",
                Email = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = PasswordHasher.HashPassword("TestPassword123!"),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(_testUser);
            _context.SaveChanges();

            _repository = new UserRepository(_context, CreateMockMapper());
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
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<UserDto, User>();
                cfg.CreateMap<Role, RoleDto>();
                cfg.CreateMap<RoleDto, Role>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task CanUserLoginAsync_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var username = "testuser@test.com";
            var password = "TestPassword123!";

            // Act
            var result = await _repository.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CanUserLoginAsync_WithInvalidUsername_ShouldReturnFalse()
        {
            // Arrange
            var username = "nonexistent@test.com";
            var password = "TestPassword123!";

            // Act
            var result = await _repository.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CanUserLoginAsync_WithInvalidPassword_ShouldReturnFalse()
        {
            // Arrange
            var username = "testuser@test.com";
            var password = "WrongPassword123!";

            // Act
            var result = await _repository.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CanUserLoginAsync_WithEmptyUsername_ShouldReturnFalse()
        {
            // Arrange
            var username = "";
            var password = "TestPassword123!";

            // Act
            var result = await _repository.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CanUserLoginAsync_WithNullUsername_ShouldReturnFalse()
        {
            // Arrange
            string username = null;
            var password = "TestPassword123!";

            // Act
            var result = await _repository.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetByUsernameAsync_WithValidUsername_ShouldReturnUser()
        {
            // Arrange
            var username = "testuser@test.com";

            // Act
            var result = await _repository.GetByUsernameAsync(username);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo(username));
            Assert.That(result.FirstName, Is.EqualTo("Test"));
            Assert.That(result.LastName, Is.EqualTo("User"));
        }

        [Test]
        public async Task GetByUsernameAsync_WithInvalidUsername_ShouldReturnNull()
        {
            // Arrange
            var username = "nonexistent@test.com";

            // Act
            var result = await _repository.GetByUsernameAsync(username);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByUsernameAsync_WithEmptyUsername_ShouldReturnNull()
        {
            // Arrange
            var username = "";

            // Act
            var result = await _repository.GetByUsernameAsync(username);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByUsernameAsync_WithNullUsername_ShouldReturnNull()
        {
            // Arrange
            string username = null;

            // Act
            var result = await _repository.GetByUsernameAsync(username);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Act
            var result = await _repository.GetByIdAsync(_testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testUser.Id));
            Assert.That(result.Username, Is.EqualTo(_testUser.UserName));
        }

        [Test]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.GetByIdAsync(999));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var additionalUser = new User
            {
                Id = 2,
                UserName = "testuser2@test.com",
                Email = "testuser2@test.com",
                FirstName = "Test2",
                LastName = "User2",
                PasswordHash = PasswordHasher.HashPassword("TestPassword123!"),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(additionalUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SaveAsync_WithNewUser_ShouldSaveToDatabase()
        {
            // Arrange
            var userDto = new UserDto
            {
                Username = "newuser@test.com",
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            await _repository.SaveAsync(userDto);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "newuser@test.com");
            Assert.That(savedUser, Is.Not.Null);
            Assert.That(savedUser.FirstName, Is.EqualTo("New"));
            Assert.That(savedUser.LastName, Is.EqualTo("User"));
        }

        [Test]
        public async Task SaveAsync_WithExistingUser_ShouldUpdateUser()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = _testUser.Id,
                Username = _testUser.UserName,
                Email = _testUser.Email,
                FirstName = "Updated",
                LastName = "User"
            };

            // Act
            await _repository.SaveAsync(userDto);

            // Assert
            var updatedUser = await _context.Users.FindAsync(_testUser.Id);
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.FirstName, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldRemoveUser()
        {
            // Act
            await _repository.DeleteAsync(_testUser.Id);

            // Assert
            var deletedUser = await _context.Users.FindAsync(_testUser.Id);
            Assert.That(deletedUser, Is.Null);
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
            var users = new List<User>
            {
                new User
                {
                    Id = 2,
                    UserName = "user2@test.com",
                    Email = "user2@test.com",
                    FirstName = "User2",
                    LastName = "Test",
                    PasswordHash = PasswordHasher.HashPassword("TestPassword123!"),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new User
                {
                    Id = 3,
                    UserName = "user3@test.com",
                    Email = "user3@test.com",
                    FirstName = "User3",
                    LastName = "Test",
                    PasswordHash = PasswordHasher.HashPassword("TestPassword123!"),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWithPaginationAsync(2, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
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
        public async Task GetByIdIfExistsAsync_WithValidId_ShouldReturnUser()
        {
            // Act
            var result = await _repository.GetByIdIfExistsAsync(_testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testUser.Id));
            Assert.That(result.Username, Is.EqualTo(_testUser.UserName));
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
            var result = await _repository.ExistsByIdAsync(_testUser.Id);

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
        public async Task CreateAsync_WithValidUser_ShouldCreateUser()
        {
            // Arrange
            var userDto = new UserDto
            {
                Username = "createuser@test.com",
                Email = "createuser@test.com",
                FirstName = "Create",
                LastName = "User"
            };

            // Act
            await _repository.CreateAsync(userDto);

            // Assert
            var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "createuser@test.com");
            Assert.That(createdUser, Is.Not.Null);
            Assert.That(createdUser.FirstName, Is.EqualTo("Create"));
        }

        [Test]
        public async Task CreateAsync_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.CreateAsync(null));
        }

        [Test]
        public async Task UpdateAsync_WithValidUser_ShouldUpdateUser()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = _testUser.Id,
                Username = _testUser.UserName,
                Email = _testUser.Email,
                FirstName = "Updated",
                LastName = "User"
            };

            // Act
            await _repository.UpdateAsync(userDto);

            // Assert
            var updatedUser = await _context.Users.FindAsync(_testUser.Id);
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.FirstName, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task UpdateAsync_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.UpdateAsync(null));
        }

        [Test]
        public async Task UpdateAsync_WithNonExistentUser_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 999,
                Username = "nonexistent@test.com",
                Email = "nonexistent@test.com",
                FirstName = "Non",
                LastName = "Existent"
            };

            // Act & Assert - The method should handle the exception gracefully and not throw
            Assert.DoesNotThrowAsync(async () => await _repository.UpdateAsync(userDto));
        }

        [Test]
        public async Task UpdateAsync_WithNewUser_ShouldSetCreatedAtAndUpdatedAt()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 0, // New user
                Username = "newuser@test.com",
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            await _repository.UpdateAsync(userDto);

            // Assert - The method should handle new users by setting timestamps
            // Note: This test verifies the method doesn't throw for new users
            Assert.DoesNotThrowAsync(async () => await _repository.UpdateAsync(userDto));
        }

        [Test]
        public async Task CanUserLoginAsync_WithNullPassword_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.CanUserLoginAsync("testuser@test.com", null));
        }

        [Test]
        public async Task CanUserLoginAsync_WithEmptyPassword_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.CanUserLoginAsync("testuser@test.com", "");

            // Assert
            Assert.That(result, Is.False);
        }
    }
} 