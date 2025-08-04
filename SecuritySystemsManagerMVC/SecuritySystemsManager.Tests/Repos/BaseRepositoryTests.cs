using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Shared.Dtos;
using NUnit.Framework;

namespace SecuritySystemsManager.Tests.Repos
{
    public abstract class BaseRepositoryTests<TRepository, T, TModel>
        where TRepository : BaseRepository<T, TModel>
        where T : class, IBaseEntity
        where TModel : BaseDto
    {
        private Mock<SecuritySystemsManagerDbContext> mockContext;
        private Mock<DbSet<T>> mockDbSet;
        private Mock<IMapper> mockMapper;
        private TRepository repository;
        
        [SetUp]
        public void Setup()
        {
            mockContext = new Mock<SecuritySystemsManagerDbContext>();
            mockDbSet = new Mock<DbSet<T>>();
            mockMapper = new Mock<IMapper>();
            repository = new Mock<TRepository>(mockContext.Object, mockMapper.Object)
            { CallBase = true }.Object;
        }
        
        [Test]
        public void MapToModel_ValidEntity_ReturnsMappedModel()
        {
            //Arrange
            var entity = new Mock<T>();
            var model = new Mock<TModel>();
            mockMapper.Setup(m => m.Map<TModel>(entity.Object)).Returns(model.Object);

            //Act
            var result = repository.MapToModel(entity.Object);

            //Assert
            Assert.That(result, Is.EqualTo(model.Object));
        }
        
        [Test]
        public void MapToEntity_ValidEntity_ReturnsMapToEntity()
        {
            //Arrange
            var entity = new Mock<T>();
            var model = new Mock<TModel>();
            mockMapper.Setup(m => m.Map<T>(model.Object)).Returns(entity.Object);

            //Act
            var result = repository.MapToEntity(model.Object);

            //Assert
            Assert.That(result, Is.EqualTo((T)entity.Object));
        }
        
        [Test]
        public void MapToEnumerableOfModel_ValidEntities_ReturnsMappedModel()
        {
            //Arrange
            var entities = new List<T> { new Mock<T>().Object };
            var model = new List<TModel> { new Mock<TModel>().Object };
            mockMapper.Setup(m => m.Map<IEnumerable<TModel>>(entities)).Returns(model);

            //Act
            var result = repository.MapToEnumerableOfModel(entities);

            //Assert
            Assert.That(result, Is.EqualTo(model));
        }
        
        [Test]
        public void Dispose_DisposesContext()
        {
            //Act
            repository.Dispose();

            //Assert
            mockContext.Verify(m => m.Dispose(), Times.Once);
        }

        [Test]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() => 
            {
                var repo = new Mock<TRepository>(null, mockMapper.Object) { CallBase = true }.Object;
            });
            Assert.That(ex.InnerException, Is.TypeOf<ArgumentNullException>());
            Assert.That(((ArgumentNullException)ex.InnerException).ParamName, Is.EqualTo("context"));
        }

        [Test]
        public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() => 
            {
                var repo = new Mock<TRepository>(mockContext.Object, null) { CallBase = true }.Object;
            });
            Assert.That(ex.InnerException, Is.TypeOf<ArgumentNullException>());
            Assert.That(((ArgumentNullException)ex.InnerException).ParamName, Is.EqualTo("mapper"));
        }

        [Test]
        public void Constructor_WithValidParameters_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                var repo = new Mock<TRepository>(mockContext.Object, mockMapper.Object) { CallBase = true }.Object;
            });
        }
    }

    // Concrete test class for BaseRepository
    [TestFixture]
    public class ConcreteBaseRepositoryTests : BaseRepositoryTests<TestRepository, TestEntity, TestDto>
    {
        // This concrete class allows us to test the abstract BaseRepository methods
    }

    // Test implementations for the concrete test class
    public class TestEntity : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TestDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestRepository : BaseRepository<TestEntity, TestDto>
    {
        public TestRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }
    }
}
