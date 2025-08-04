using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    [TestFixture]
    public class RoleServiceTests : BaseServiceTests<RoleDto, IRoleRepository, RoleService>
    {
        protected override RoleService CreateService(IRoleRepository repository)
        {
            return new RoleService(repository);
        }

        protected override RoleDto CreateTestModel(int id = 1)
        {
            return new RoleDto
            {
                Id = id,
                Name = $"Role {id}",
                RoleType = (SecuritySystemsManager.Shared.Enums.RoleType)id
            };
        }

        [Test]
        public async Task WhenSaveAsync_WithValidRoleData_ThenSaveAsync()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Name = "Admin",
                RoleType = SecuritySystemsManager.Shared.Enums.RoleType.Admin
            };

            // Act
            await _service.SaveAsync(roleDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(roleDto), Times.Once());
        }

        [Test]
        public async Task WhenSaveAsync_WithNull_ThenThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<System.ArgumentNullException>(async () => await _service.SaveAsync(null));

            _mockRepository.Verify(x => x.SaveAsync(null), Times.Never);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(22)]
        [TestCase(131)]
        public async Task WhenDeleteAsync_WithCorrectId_ThenCallDeleteAsyncInRepository(int id)
        {
            // Act
            await _service.DeleteAsync(id);

            // Assert
            _mockRepository.Verify(x => x.DeleteAsync(It.Is<int>(i => i.Equals(id))), Times.Once);
        }

        [Theory]
        [TestCase(1)]
        [TestCase(22)]
        [TestCase(131)]
        public async Task WhenGetByIdAsync_WithValidRoleId_ThenReturnRole(int roleId)
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Id = roleId,
                Name = "Manager",
                RoleType = SecuritySystemsManager.Shared.Enums.RoleType.Manager
            };

            _mockRepository.Setup(x => x.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(roleId))))
                .ReturnsAsync(roleDto);

            // Act
            var roleResult = await _service.GetByIdIfExistsAsync(roleId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(roleId), Times.Once());
            Assert.That(roleResult, Is.EqualTo(roleDto));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(102021)]
        public async Task WhenGetByIdAsync_WithInvalidRoleId_ThenReturnDefault(int roleId)
        {
            // Arrange
            var role = (RoleDto)default;

            _mockRepository.Setup(s => s.GetByIdIfExistsAsync(It.Is<int>(x => x.Equals(roleId))))
                .ReturnsAsync(role);

            // Act
            var roleResult = await _service.GetByIdIfExistsAsync(roleId);

            // Assert
            _mockRepository.Verify(x => x.GetByIdIfExistsAsync(roleId), Times.Once());
            Assert.That(roleResult, Is.EqualTo(role));
        }

        [Test]
        public async Task WhenUpdateAsync_WithValidData_ThenSaveAsync()
        {
            // Arrange
            var roleDto = new RoleDto
            {
                Id = 1,
                Name = "Updated Admin",
                RoleType = SecuritySystemsManager.Shared.Enums.RoleType.Admin
            };

            _mockRepository.Setup(s => s.SaveAsync(It.Is<RoleDto>(x => x.Equals(roleDto))))
                .Verifiable();

            // Act
            await _service.SaveAsync(roleDto);

            // Assert
            _mockRepository.Verify(x => x.SaveAsync(roleDto), Times.Once());
        }

        [Test]
        public async Task WhenGetAllAsync_ThenReturnAllRoles()
        {
            // Arrange
            var roleList = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin", RoleType = SecuritySystemsManager.Shared.Enums.RoleType.Admin },
                new RoleDto { Id = 2, Name = "Manager", RoleType = SecuritySystemsManager.Shared.Enums.RoleType.Manager },
                new RoleDto { Id = 3, Name = "Technician", RoleType = SecuritySystemsManager.Shared.Enums.RoleType.Technician }
            };

            _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(roleList);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            _mockRepository.Verify(x => x.GetAllAsync(), Times.Once());
            Assert.That(result, Is.EqualTo(roleList));
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WhenRoleExists_ReturnsRole()
        {
            // Arrange
            var roleName = "Admin";
            var expectedRole = CreateTestModel(1);
            expectedRole.Name = roleName;

            _mockRepository.Setup(r => r.GetByNameIfExistsAsync(roleName))
                .ReturnsAsync(expectedRole);

            // Act
            var result = await _service.GetByNameIfExistsAsync(roleName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(roleName));
            _mockRepository.Verify(r => r.GetByNameIfExistsAsync(roleName), Times.Once);
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WhenRoleDoesNotExist_ReturnsNull()
        {
            // Arrange
            var roleName = "NonExistentRole";

            _mockRepository.Setup(r => r.GetByNameIfExistsAsync(roleName))
                .ReturnsAsync((RoleDto)null);

            // Act
            var result = await _service.GetByNameIfExistsAsync(roleName);

            // Assert
            Assert.That(result, Is.Null);
            _mockRepository.Verify(r => r.GetByNameIfExistsAsync(roleName), Times.Once);
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WithEmptyName_ReturnsNull()
        {
            // Arrange
            var roleName = "";

            _mockRepository.Setup(r => r.GetByNameIfExistsAsync(roleName))
                .ReturnsAsync((RoleDto)null);

            // Act
            var result = await _service.GetByNameIfExistsAsync(roleName);

            // Assert
            Assert.That(result, Is.Null);
            _mockRepository.Verify(r => r.GetByNameIfExistsAsync(roleName), Times.Once);
        }

        [Test]
        public async Task GetByNameIfExistsAsync_WithNullName_ReturnsNull()
        {
            // Arrange
            string roleName = null;

            _mockRepository.Setup(r => r.GetByNameIfExistsAsync(roleName))
                .ReturnsAsync((RoleDto)null);

            // Act
            var result = await _service.GetByNameIfExistsAsync(roleName);

            // Assert
            Assert.That(result, Is.Null);
            _mockRepository.Verify(r => r.GetByNameIfExistsAsync(roleName), Times.Once);
        }

        [Theory]
        [TestCase("Admin")]
        [TestCase("Manager")]
        [TestCase("Technician")]
        [TestCase("Client")]
        public async Task GetByNameIfExistsAsync_WithValidRoleNames_ShouldCallRepository(string roleName)
        {
            // Arrange
            var expectedRole = new RoleDto { Name = roleName };
            _mockRepository.Setup(r => r.GetByNameIfExistsAsync(roleName))
                .ReturnsAsync(expectedRole);

            // Act
            var result = await _service.GetByNameIfExistsAsync(roleName);

            // Assert
            _mockRepository.Verify(r => r.GetByNameIfExistsAsync(roleName), Times.Once);
            Assert.That(result, Is.EqualTo(expectedRole));
        }

        [Test]
        public async Task WhenExistsByIdAsync_WithValidId_ThenReturnTrue()
        {
            // Arrange
            int id = 1;
            _mockRepository.Setup(x => x.ExistsByIdAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _service.ExistsByIdAsync(id);

            // Assert
            Assert.That(result, Is.True);
            _mockRepository.Verify(x => x.ExistsByIdAsync(id), Times.Once());
        }

        [Test]
        public async Task WhenExistsByIdAsync_WithInvalidId_ThenReturnFalse()
        {
            // Arrange
            int id = 999;
            _mockRepository.Setup(x => x.ExistsByIdAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _service.ExistsByIdAsync(id);

            // Assert
            Assert.That(result, Is.False);
            _mockRepository.Verify(x => x.ExistsByIdAsync(id), Times.Once());
        }

        [Theory]
        [TestCase(10, 1)]
        [TestCase(20, 2)]
        [TestCase(5, 3)]
        public async Task WhenGetWithPaginationAsync_WithValidParameters_ThenReturnPaginatedRoles(int pageSize, int pageNumber)
        {
            // Arrange
            var roleList = new List<RoleDto>
            {
                new RoleDto { Id = 1, Name = "Admin" },
                new RoleDto { Id = 2, Name = "Manager" }
            };

            _mockRepository.Setup(x => x.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(roleList);

            // Act
            var result = await _service.GetWithPaginationAsync(pageSize, pageNumber);

            // Assert
            _mockRepository.Verify(x => x.GetWithPaginationAsync(pageSize, pageNumber), Times.Once());
            Assert.That(result, Is.EqualTo(roleList));
        }
    }
} 