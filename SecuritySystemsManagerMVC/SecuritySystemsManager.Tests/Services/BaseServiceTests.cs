using Moq;
using NUnit.Framework;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Tests.Services
{
    public abstract class BaseServiceTests<TModel, TRepository, TService>
        where TModel : BaseDto, new()
        where TRepository : class, IBaseRepository<TModel>
        where TService : BaseCrudService<TModel, TRepository>
    {
        protected Mock<TRepository> _mockRepository;
        protected TService _service;

        [SetUp]
        public virtual void Setup()
        {
            _mockRepository = new Mock<TRepository>();
            _service = CreateService(_mockRepository.Object);
        }

        protected abstract TService CreateService(TRepository repository);

        protected virtual TModel CreateTestModel(int id = 1)
        {
            return new TModel { Id = id };
        }

        protected virtual List<TModel> CreateTestModels(int count = 3)
        {
            return Enumerable.Range(1, count)
                .Select(i => CreateTestModel(i))
                .ToList();
        }

        [Test]
        public async Task SaveAsync_WithValidModel_ShouldCallRepository()
        {
            // Arrange
            var model = CreateTestModel();
            _mockRepository.Setup(r => r.SaveAsync(model)).Returns(Task.CompletedTask);

            // Act
            await _service.SaveAsync(model);

            // Assert
            _mockRepository.Verify(r => r.SaveAsync(model), Times.Once);
        }

        [Test]
        public void SaveAsync_WithNullModel_ShouldThrowArgumentNullException()
        {
            // Arrange
            TModel model = null;

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => _service.SaveAsync(model));
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllModels()
        {
            // Arrange
            var expectedModels = CreateTestModels();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedModels);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.That(result, Is.EqualTo(expectedModels));
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WithValidId_ShouldCallRepository()
        {
            // Arrange
            int id = 1;
            _mockRepository.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(id);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Test]
        public async Task GetByIdIfExistsAsync_WithValidId_ShouldReturnModel()
        {
            // Arrange
            int id = 1;
            var expectedModel = CreateTestModel(id);
            _mockRepository.Setup(r => r.GetByIdIfExistsAsync(id)).ReturnsAsync(expectedModel);

            // Act
            var result = await _service.GetByIdIfExistsAsync(id);

            // Assert
            Assert.That(result, Is.EqualTo(expectedModel));
            _mockRepository.Verify(r => r.GetByIdIfExistsAsync(id), Times.Once);
        }

        [Test]
        public async Task GetWithPaginationAsync_WithValidParameters_ShouldReturnPaginatedModels()
        {
            // Arrange
            int pageSize = 10;
            int pageNumber = 1;
            var expectedModels = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedModels);

            // Act
            var result = await _service.GetWithPaginationAsync(pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedModels));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task ExistsByIdAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            int id = 1;
            _mockRepository.Setup(r => r.ExistsByIdAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _service.ExistsByIdAsync(id);

            // Assert
            Assert.That(result, Is.True);
            _mockRepository.Verify(r => r.ExistsByIdAsync(id), Times.Once);
        }

        [Test]
        public async Task ExistsByIdAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            int id = 999;
            _mockRepository.Setup(r => r.ExistsByIdAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _service.ExistsByIdAsync(id);

            // Assert
            Assert.That(result, Is.False);
            _mockRepository.Verify(r => r.ExistsByIdAsync(id), Times.Once);
        }
    }
}
