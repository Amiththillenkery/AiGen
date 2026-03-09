using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using FluentValidation;

namespace Tests
{
    public class CreatePaymentEntityServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IValidator<Payment>> _paymentValidatorMock;
        private readonly PaymentService _paymentService;

        public CreatePaymentEntityServiceTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _paymentValidatorMock = new Mock<IValidator<Payment>>();
            _paymentService = new PaymentService(_paymentRepositoryMock.Object, _paymentValidatorMock.Object);
        }

        [Fact]
        public async Task CreatePayment_Should_SavePayment_When_ValidPayment()
        {
            // Arrange
            var payment = new Payment { Id = Guid.NewGuid(), Amount = 100, Description = "Test Payment" };
            _paymentValidatorMock.Setup(v => v.ValidateAsync(payment, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Act
            await _paymentService.CreatePaymentAsync(payment);

            // Assert
            _paymentRepositoryMock.Verify(r => r.AddAsync(payment), Times.Once);
        }

        [Fact]
        public async Task CreatePayment_Should_ThrowValidationException_When_InvalidPayment()
        {
            // Arrange
            var payment = new Payment { Id = Guid.NewGuid(), Amount = -100, Description = "Invalid Payment" };
            var validationResult = new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Amount", "Amount must be positive") });
            _paymentValidatorMock.Setup(v => v.ValidateAsync(payment, default)).ReturnsAsync(validationResult);

            // Act
            Func<Task> act = async () => await _paymentService.CreatePaymentAsync(payment);

            // Assert
            await act.Should().ThrowAsync<ValidationException>().WithMessage("Amount must be positive");
        }

        [Fact]
        public async Task CreatePayment_Should_HandleDatabaseException_Gracefully()
        {
            // Arrange
            var payment = new Payment { Id = Guid.NewGuid(), Amount = 100, Description = "Test Payment" };
            _paymentValidatorMock.Setup(v => v.ValidateAsync(payment, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _paymentRepositoryMock.Setup(r => r.AddAsync(payment)).ThrowsAsync(new DbUpdateException("Database unavailable"));

            // Act
            Func<Task> act = async () => await _paymentService.CreatePaymentAsync(payment);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>().WithMessage("Database unavailable");
        }
    }
}