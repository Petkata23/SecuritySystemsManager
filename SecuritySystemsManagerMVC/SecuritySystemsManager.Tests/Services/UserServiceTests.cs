using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SecuritySystemsManager.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class UserServiceTests : BaseServiceTests<UserDto, IUserRepository, UserService>
    {
        private Mock<IFileStorageService> _mockFileStorageService;
        private Mock<UserManager<User>> _mockUserManager;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);
            _service = new UserService(_mockRepository.Object, _mockFileStorageService.Object, _mockUserManager.Object);
        }

        protected override UserService CreateService(IUserRepository repository)
        {
            return new UserService(repository, _mockFileStorageService?.Object ?? new Mock<IFileStorageService>().Object, _mockUserManager?.Object ?? new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null).Object);
        }

        protected override UserDto CreateTestModel(int id = 1)
        {
            return new UserDto
            {
                Id = id,
                Username = $"user{id}",
                Email = $"user{id}@test.com",
                FirstName = $"First{id}",
                LastName = $"Last{id}",
                ProfileImage = $"profile_{id}.jpg",
                RoleId = 1
            };
        }

        [Test]
        public async Task CanUserLoginAsync_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            string username = "testuser";
            string password = "password123";
            var user = CreateTestModel();
            _mockRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.FindByNameAsync(username)).ReturnsAsync(new User { UserName = username });
            _mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<User>(), password)).ReturnsAsync(true);

            // Act
            var result = await _service.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.True);
            _mockRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
            _mockUserManager.Verify(m => m.FindByNameAsync(username), Times.Once);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(It.IsAny<User>(), password), Times.Once);
        }

        [Test]
        public async Task CanUserLoginAsync_WithInvalidUsername_ShouldReturnFalse()
        {
            // Arrange
            string username = "nonexistent";
            string password = "password123";
            _mockRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync((UserDto)null);

            // Act
            var result = await _service.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.False);
            _mockRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
        }

        [Test]
        public async Task CanUserLoginAsync_WithInvalidPassword_ShouldReturnFalse()
        {
            // Arrange
            string username = "testuser";
            string password = "wrongpassword";
            var user = CreateTestModel();
            _mockRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.FindByNameAsync(username)).ReturnsAsync(new User { UserName = username });
            _mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<User>(), password)).ReturnsAsync(false);

            // Act
            var result = await _service.CanUserLoginAsync(username, password);

            // Assert
            Assert.That(result, Is.False);
            _mockRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
            _mockUserManager.Verify(m => m.FindByNameAsync(username), Times.Once);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(It.IsAny<User>(), password), Times.Once);
        }

        [Test]
        public async Task GetByUsernameAsync_WithValidUsername_ShouldReturnUser()
        {
            // Arrange
            string username = "testuser";
            var expectedUser = CreateTestModel();
            _mockRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(expectedUser);

            // Act
            var result = await _service.GetByUsernameAsync(username);

            // Assert
            Assert.That(result, Is.EqualTo(expectedUser));
            _mockRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
        }

        [Test]
        public async Task CreateUserWithPasswordAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var userDto = CreateTestModel();
            string password = "password123";
            var user = new User { Id = userDto.Id, UserName = userDto.Username };
            var identityResult = IdentityResult.Success;
            
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), password)).ReturnsAsync(identityResult);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.FindByNameAsync(userDto.Username)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.GetByIdAsync(userDto.Id)).ReturnsAsync(userDto);

            // Act
            var result = await _service.CreateUserWithPasswordAsync(userDto, password);

            // Assert
            Assert.That(result, Is.EqualTo(userDto));
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<User>(), password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _mockRepository.Verify(r => r.GetByIdAsync(userDto.Id), Times.Once);
        }

        [Test]
        public void CreateUserWithPasswordAsync_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var userDto = CreateTestModel();
            string password = "password123";
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "User creation failed" });
            
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), password)).ReturnsAsync(identityResult);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserWithPasswordAsync(userDto, password));
            Assert.That(exception.Message, Does.Contain("Failed to create user"));
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<User>(), password), Times.Once);
        }

        [Test]
        public async Task UpdateUserWithPasswordAsync_WithValidData_ShouldUpdateUser()
        {
            // Arrange
            var userDto = CreateTestModel();
            string password = "newpassword123";
            var user = new User { Id = userDto.Id, UserName = userDto.Username };
            
            _mockUserManager.Setup(m => m.FindByIdAsync(userDto.Id.ToString())).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            _mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset_token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, It.IsAny<string>(), password)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _mockRepository.Setup(r => r.GetByIdAsync(userDto.Id)).ReturnsAsync(userDto);

            // Act
            var result = await _service.UpdateUserWithPasswordAsync(userDto, password);

            // Assert
            Assert.That(result, Is.EqualTo(userDto));
            _mockUserManager.Verify(m => m.FindByIdAsync(userDto.Id.ToString()), Times.Once);
            _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);
            _mockRepository.Verify(r => r.GetByIdAsync(userDto.Id), Times.Once);
        }

        [Test]
        public async Task UpdateUserWithPasswordAsync_WithoutPassword_ShouldUpdateUserOnly()
        {
            // Arrange
            var userDto = CreateTestModel();
            var user = new User { Id = userDto.Id, UserName = userDto.Username };
            
            _mockUserManager.Setup(m => m.FindByIdAsync(userDto.Id.ToString())).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            _mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _mockRepository.Setup(r => r.GetByIdAsync(userDto.Id)).ReturnsAsync(userDto);

            // Act
            var result = await _service.UpdateUserWithPasswordAsync(userDto, null);

            // Assert
            Assert.That(result, Is.EqualTo(userDto));
            _mockUserManager.Verify(m => m.FindByIdAsync(userDto.Id.ToString()), Times.Once);
            _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);
            _mockRepository.Verify(r => r.GetByIdAsync(userDto.Id), Times.Once);
        }

        [Test]
        public async Task UploadUserProfileImageAsync_WithValidFile_ShouldReturnImagePath()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string expectedPath = "uploads/profiles/profile.jpg";
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/profiles")).ReturnsAsync(expectedPath);

            // Act
            var result = await _service.UploadUserProfileImageAsync(mockFile.Object);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPath));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(mockFile.Object, "uploads/profiles"), Times.Once);
        }

        [Test]
        public async Task UploadUserProfileImageAsync_WithNullFile_ShouldReturnNull()
        {
            // Act
            var result = await _service.UploadUserProfileImageAsync(null);

            // Assert
            Assert.That(result, Is.Null);
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UploadUserProfileImageAsync_WithEmptyFile_ShouldReturnNull()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _service.UploadUserProfileImageAsync(mockFile.Object);

            // Assert
            Assert.That(result, Is.Null);
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UploadProfileImageAsync_WithValidData_ShouldUpdateUserProfile()
        {
            // Arrange
            int userId = 1;
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string expectedPath = "uploads/profiles/profile.jpg";
            var user = CreateTestModel(userId);
            
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/profiles")).ReturnsAsync(expectedPath);
            _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<UserDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UploadProfileImageAsync(userId, mockFile.Object);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPath));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(mockFile.Object, "uploads/profiles"), Times.Once);
            _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(user), Times.Once);
        }

        [Test]
        public async Task CreateUserWithDetailsAsync_WithValidData_ShouldCreateUserWithImage()
        {
            // Arrange
            var userDto = CreateTestModel();
            string password = "password123";
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            string expectedPath = "uploads/profiles/profile.jpg";
            var user = new User { Id = userDto.Id, UserName = userDto.Username };
            var identityResult = IdentityResult.Success;
            
            _mockFileStorageService.Setup(f => f.UploadFileAsync(mockFile.Object, "uploads/profiles")).ReturnsAsync(expectedPath);
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), password)).ReturnsAsync(identityResult);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.FindByNameAsync(userDto.Username)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.GetByIdAsync(userDto.Id)).ReturnsAsync(userDto);

            // Act
            var result = await _service.CreateUserWithDetailsAsync(userDto, password, mockFile.Object);

            // Assert
            Assert.That(result, Is.EqualTo(userDto));
            Assert.That(result.ProfileImage, Is.EqualTo(expectedPath));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(mockFile.Object, "uploads/profiles"), Times.Once);
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<User>(), password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _mockRepository.Verify(r => r.GetByIdAsync(userDto.Id), Times.Once);
        }

        [Test]
        public async Task CreateUserWithDetailsAsync_WithoutImage_ShouldCreateUserOnly()
        {
            // Arrange
            var userDto = CreateTestModel();
            string password = "password123";
            var user = new User { Id = userDto.Id, UserName = userDto.Username };
            var identityResult = IdentityResult.Success;
            
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), password)).ReturnsAsync(identityResult);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.FindByNameAsync(userDto.Username)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.GetByIdAsync(userDto.Id)).ReturnsAsync(userDto);

            // Act
            var result = await _service.CreateUserWithDetailsAsync(userDto, password, null);

            // Assert
            Assert.That(result, Is.EqualTo(userDto));
            _mockFileStorageService.Verify(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<User>(), password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _mockRepository.Verify(r => r.GetByIdAsync(userDto.Id), Times.Once);
        }
    }
} 