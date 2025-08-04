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
    public class InvoiceServiceTests : BaseServiceTests<InvoiceDto, IInvoiceRepository, InvoiceService>
    {
        protected override InvoiceService CreateService(IInvoiceRepository repository)
        {
            return new InvoiceService(repository);
        }

        protected override InvoiceDto CreateTestModel(int id = 1)
        {
            return new InvoiceDto
            {
                Id = id,
                SecuritySystemOrderId = id,
                IssuedOn = System.DateTime.UtcNow,
                TotalAmount = 1000.0m,
                IsPaid = false
            };
        }

        [Test]
        public async Task GetInvoiceWithDetailsAsync_WithValidId_ShouldReturnInvoiceWithDetails()
        {
            // Arrange
            int invoiceId = 1;
            var expectedInvoice = CreateTestModel(invoiceId);
            _mockRepository.Setup(r => r.GetInvoiceWithDetailsAsync(invoiceId)).ReturnsAsync(expectedInvoice);

            // Act
            var result = await _service.GetInvoiceWithDetailsAsync(invoiceId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedInvoice));
            _mockRepository.Verify(r => r.GetInvoiceWithDetailsAsync(invoiceId), Times.Once);
        }

        [Test]
        public async Task GetInvoiceWithDetailsAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            int invoiceId = 999;
            _mockRepository.Setup(r => r.GetInvoiceWithDetailsAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act
            var result = await _service.GetInvoiceWithDetailsAsync(invoiceId);

            // Assert
            Assert.That(result, Is.Null);
            _mockRepository.Verify(r => r.GetInvoiceWithDetailsAsync(invoiceId), Times.Once);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithAdminRole_ShouldReturnAllInvoices()
        {
            // Arrange
            int userId = 1;
            string userRole = "Admin";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedInvoices = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedInvoices);

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedInvoices));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithManagerRole_ShouldReturnAllInvoices()
        {
            // Arrange
            int userId = 1;
            string userRole = "Manager";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedInvoices = CreateTestModels();
            _mockRepository.Setup(r => r.GetWithPaginationAsync(pageSize, pageNumber)).ReturnsAsync(expectedInvoices);

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedInvoices));
            _mockRepository.Verify(r => r.GetWithPaginationAsync(pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithClientRole_ShouldReturnClientInvoices()
        {
            // Arrange
            int userId = 1;
            string userRole = "Client";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedInvoices = CreateTestModels();
            _mockRepository.Setup(r => r.GetInvoicesByClientIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedInvoices);

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedInvoices));
            _mockRepository.Verify(r => r.GetInvoicesByClientIdAsync(userId, pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithTechnicianRole_ShouldReturnTechnicianInvoices()
        {
            // Arrange
            int userId = 1;
            string userRole = "Technician";
            int pageSize = 10;
            int pageNumber = 1;
            var expectedInvoices = CreateTestModels();
            _mockRepository.Setup(r => r.GetInvoicesByTechnicianIdAsync(userId, pageSize, pageNumber)).ReturnsAsync(expectedInvoices);

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.EqualTo(expectedInvoices));
            _mockRepository.Verify(r => r.GetInvoicesByTechnicianIdAsync(userId, pageSize, pageNumber), Times.Once);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithUnknownRole_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            string userRole = "Unknown";
            int pageSize = 10;
            int pageNumber = 1;

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithEmptyRole_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            string userRole = "";
            int pageSize = 10;
            int pageNumber = 1;

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetInvoicesByUserRoleAsync_WithNullRole_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 1;
            string userRole = null;
            int pageSize = 10;
            int pageNumber = 1;

            // Act
            var result = await _service.GetInvoicesByUserRoleAsync(userId, userRole, pageSize, pageNumber);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithAdminRole_ShouldReturnAllInvoicesCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Admin";
            var allInvoices = CreateTestModels();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allInvoices);

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithManagerRole_ShouldReturnAllInvoicesCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Manager";
            var allInvoices = CreateTestModels();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allInvoices);

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(3));
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithClientRole_ShouldReturnClientInvoicesCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Client";
            int expectedCount = 5;
            _mockRepository.Setup(r => r.GetInvoicesCountByClientIdAsync(userId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
            _mockRepository.Verify(r => r.GetInvoicesCountByClientIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithTechnicianRole_ShouldReturnTechnicianInvoicesCount()
        {
            // Arrange
            int userId = 1;
            string userRole = "Technician";
            int expectedCount = 3;
            _mockRepository.Setup(r => r.GetInvoicesCountByTechnicianIdAsync(userId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
            _mockRepository.Verify(r => r.GetInvoicesCountByTechnicianIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithUnknownRole_ShouldReturnZero()
        {
            // Arrange
            int userId = 1;
            string userRole = "Unknown";

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithEmptyRole_ShouldReturnZero()
        {
            // Arrange
            int userId = 1;
            string userRole = "";

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetInvoicesCountByUserRoleAsync_WithNullRole_ShouldReturnZero()
        {
            // Arrange
            int userId = 1;
            string userRole = null;

            // Act
            var result = await _service.GetInvoicesCountByUserRoleAsync(userId, userRole);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task MarkAsPaidAsync_WithValidId_ShouldMarkInvoiceAsPaid()
        {
            // Arrange
            int invoiceId = 1;
            var invoice = CreateTestModel(invoiceId);
            invoice.IsPaid = false;
            
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.MarkAsPaidAsync(invoiceId);

            // Assert
            Assert.That(result.IsPaid, Is.True);
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(invoice), Times.Once);
        }

        [Test]
        public async Task MarkAsPaidAsync_WithAlreadyPaidInvoice_ShouldKeepInvoiceAsPaid()
        {
            // Arrange
            int invoiceId = 1;
            var invoice = CreateTestModel(invoiceId);
            invoice.IsPaid = true; // Already paid
            
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.MarkAsPaidAsync(invoiceId);

            // Assert
            Assert.That(result.IsPaid, Is.True);
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(invoice), Times.Once);
        }

        [Test]
        public void MarkAsPaidAsync_WithInvalidId_ShouldThrowArgumentException()
        {
            // Arrange
            int invoiceId = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsPaidAsync(invoiceId));
            Assert.That(exception.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
        }

        [Test]
        public void MarkAsPaidAsync_WithZeroId_ShouldThrowArgumentException()
        {
            // Arrange
            int invoiceId = 0;
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsPaidAsync(invoiceId));
            Assert.That(exception.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
        }

        [Test]
        public void MarkAsPaidAsync_WithNegativeId_ShouldThrowArgumentException()
        {
            // Arrange
            int invoiceId = -1;
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsPaidAsync(invoiceId));
            Assert.That(exception.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
        }

        [Test]
        public async Task MarkAsUnpaidAsync_WithValidId_ShouldMarkInvoiceAsUnpaid()
        {
            // Arrange
            int invoiceId = 1;
            var invoice = CreateTestModel(invoiceId);
            invoice.IsPaid = true;
            
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.MarkAsUnpaidAsync(invoiceId);

            // Assert
            Assert.That(result.IsPaid, Is.False);
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(invoice), Times.Once);
        }

        [Test]
        public async Task MarkAsUnpaidAsync_WithAlreadyUnpaidInvoice_ShouldKeepInvoiceAsUnpaid()
        {
            // Arrange
            int invoiceId = 1;
            var invoice = CreateTestModel(invoiceId);
            invoice.IsPaid = false; // Already unpaid
            
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.MarkAsUnpaidAsync(invoiceId);

            // Assert
            Assert.That(result.IsPaid, Is.False);
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(invoice), Times.Once);
        }

        [Test]
        public void MarkAsUnpaidAsync_WithInvalidId_ShouldThrowArgumentException()
        {
            // Arrange
            int invoiceId = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsUnpaidAsync(invoiceId));
            Assert.That(exception.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
        }

        [Test]
        public void MarkAsUnpaidAsync_WithZeroId_ShouldThrowArgumentException()
        {
            // Arrange
            int invoiceId = 0;
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsUnpaidAsync(invoiceId));
            Assert.That(exception.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
        }

        [Test]
        public void MarkAsUnpaidAsync_WithNegativeId_ShouldThrowArgumentException()
        {
            // Arrange
            int invoiceId = -1;
            _mockRepository.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync((InvoiceDto)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsUnpaidAsync(invoiceId));
            Assert.That(exception.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
            _mockRepository.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
        }

        [Test]
        public async Task GenerateInvoiceFromOrderAsync_WithValidOrderId_ShouldGenerateInvoice()
        {
            // Arrange
            int orderId = 100; // Use unique order ID
            decimal totalAmount = 1000.0m;
            var generatedInvoice = CreateTestModel();
            generatedInvoice.SecuritySystemOrderId = orderId;
            generatedInvoice.TotalAmount = totalAmount;
            
            var sequence = new MockSequence();
            _mockRepository.InSequence(sequence).Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync((InvoiceDto)null);
            _mockRepository.InSequence(sequence).Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync(generatedInvoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GenerateInvoiceFromOrderAsync(orderId, totalAmount);

            // Assert
            Assert.That(result.SecuritySystemOrderId, Is.EqualTo(orderId));
            Assert.That(result.TotalAmount, Is.EqualTo(totalAmount));
            Assert.That(result.IsPaid, Is.False);
            _mockRepository.Verify(r => r.GetInvoiceByOrderIdAsync(orderId), Times.Exactly(2));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<InvoiceDto>()), Times.Once);
        }

        [Test]
        public async Task GenerateInvoiceFromOrderAsync_WithZeroAmount_ShouldGenerateInvoice()
        {
            // Arrange
            int orderId = 101; // Use unique order ID
            decimal totalAmount = 0.0m;
            var generatedInvoice = CreateTestModel();
            generatedInvoice.SecuritySystemOrderId = orderId;
            generatedInvoice.TotalAmount = totalAmount;
            
            var sequence = new MockSequence();
            _mockRepository.InSequence(sequence).Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync((InvoiceDto)null);
            _mockRepository.InSequence(sequence).Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync(generatedInvoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GenerateInvoiceFromOrderAsync(orderId, totalAmount);

            // Assert
            Assert.That(result.SecuritySystemOrderId, Is.EqualTo(orderId));
            Assert.That(result.TotalAmount, Is.EqualTo(totalAmount));
            Assert.That(result.IsPaid, Is.False);
            _mockRepository.Verify(r => r.GetInvoiceByOrderIdAsync(orderId), Times.Exactly(2));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<InvoiceDto>()), Times.Once);
        }

        [Test]
        public async Task GenerateInvoiceFromOrderAsync_WithNegativeAmount_ShouldGenerateInvoice()
        {
            // Arrange
            int orderId = 102; // Use unique order ID
            decimal totalAmount = -100.0m;
            var generatedInvoice = CreateTestModel();
            generatedInvoice.SecuritySystemOrderId = orderId;
            generatedInvoice.TotalAmount = totalAmount;
            
            var sequence = new MockSequence();
            _mockRepository.InSequence(sequence).Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync((InvoiceDto)null);
            _mockRepository.InSequence(sequence).Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync(generatedInvoice);
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<InvoiceDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GenerateInvoiceFromOrderAsync(orderId, totalAmount);

            // Assert
            Assert.That(result.SecuritySystemOrderId, Is.EqualTo(orderId));
            Assert.That(result.TotalAmount, Is.EqualTo(totalAmount));
            Assert.That(result.IsPaid, Is.False);
            _mockRepository.Verify(r => r.GetInvoiceByOrderIdAsync(orderId), Times.Exactly(2));
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<InvoiceDto>()), Times.Once);
        }

        [Test]
        public void GenerateInvoiceFromOrderAsync_WithExistingInvoice_ShouldThrowInvalidOperationException()
        {
            // Arrange
            int orderId = 1;
            decimal totalAmount = 1000.0m;
            var existingInvoice = CreateTestModel();
            
            _mockRepository.Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync(existingInvoice);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateInvoiceFromOrderAsync(orderId, totalAmount));
            Assert.That(exception.Message, Does.Contain($"Invoice already exists for order {orderId}"));
            _mockRepository.Verify(r => r.GetInvoiceByOrderIdAsync(orderId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<InvoiceDto>()), Times.Never);
        }

        [Test]
        public void GenerateInvoiceFromOrderAsync_WithZeroOrderId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            int orderId = 0;
            decimal totalAmount = 1000.0m;
            var existingInvoice = CreateTestModel();
            
            _mockRepository.Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync(existingInvoice);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateInvoiceFromOrderAsync(orderId, totalAmount));
            Assert.That(exception.Message, Does.Contain($"Invoice already exists for order {orderId}"));
            _mockRepository.Verify(r => r.GetInvoiceByOrderIdAsync(orderId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<InvoiceDto>()), Times.Never);
        }

        [Test]
        public void GenerateInvoiceFromOrderAsync_WithNegativeOrderId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            int orderId = -1;
            decimal totalAmount = 1000.0m;
            var existingInvoice = CreateTestModel();
            
            _mockRepository.Setup(r => r.GetInvoiceByOrderIdAsync(orderId)).ReturnsAsync(existingInvoice);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateInvoiceFromOrderAsync(orderId, totalAmount));
            Assert.That(exception.Message, Does.Contain($"Invoice already exists for order {orderId}"));
            _mockRepository.Verify(r => r.GetInvoiceByOrderIdAsync(orderId), Times.Once);
            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<InvoiceDto>()), Times.Never);
        }
    }
} 