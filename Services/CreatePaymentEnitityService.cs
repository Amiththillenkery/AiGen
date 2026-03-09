using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FluentValidation;
using AutoMapper;
using @/Domain.Entities;
using @/Domain.Interfaces;
using @/Application.DTOs;
using @/Application.Interfaces;
using @/Application.Validators;

namespace @/Services
{
    public class CreatePaymentEntityService : ICreatePaymentEntityService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<PaymentDto> _validator;
        private readonly ILogger<CreatePaymentEntityService> _logger;

        public CreatePaymentEntityService(
            IApplicationDbContext context,
            IMapper mapper,
            IValidator<PaymentDto> validator,
            ILogger<CreatePaymentEntityService> logger)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result> CreatePaymentAsync(PaymentDto paymentDto)
        {
            try
            {
                var validationResult = _validator.Validate(paymentDto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validation failed for payment creation: {Errors}", validationResult.Errors);
                    return Result.Failure(validationResult.Errors);
                }

                var paymentEntity = _mapper.Map<Payment>(paymentDto);
                await _context.Payments.AddAsync(paymentEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment entity created successfully with ID: {PaymentId}", paymentEntity.Id);
                return Result.Success();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update exception occurred while creating payment entity.");
                return Result.Failure("An error occurred while saving the payment entity. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating payment entity.");
                return Result.Failure("An unexpected error occurred. Please try again later.");
            }
        }
    }
}