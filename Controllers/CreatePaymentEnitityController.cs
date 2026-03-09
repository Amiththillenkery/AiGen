using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AutoMapper;
using @/Domain.Entities;
using @/Application.Interfaces;
using @/Application.Models;
using @/Application.Validators;

namespace @/Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreatePaymentEntityController : ControllerBase
    {
        private readonly ILogger<CreatePaymentEntityController> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<PaymentDto> _validator;

        public CreatePaymentEntityController(
            ILogger<CreatePaymentEntityController> logger,
            IApplicationDbContext context,
            IMapper mapper,
            IValidator<PaymentDto> validator)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentDto paymentDto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(paymentDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var paymentEntity = _mapper.Map<Payment>(paymentDto);
            await _context.Payments.AddAsync(paymentEntity, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return CreatedAtAction(nameof(GetPaymentById), new { id = paymentEntity.Id }, paymentEntity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while saving the payment entity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id, CancellationToken cancellationToken)
        {
            var paymentEntity = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
            if (paymentEntity == null)
            {
                return NotFound();
            }

            return Ok(paymentEntity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentDto paymentDto, CancellationToken cancellationToken)
        {
            if (id != paymentDto.Id)
            {
                return BadRequest("Payment ID mismatch.");
            }

            var validationResult = await _validator.ValidateAsync(paymentDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var paymentEntity = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
            if (paymentEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(paymentDto, paymentEntity);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating the payment entity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
        {
            var paymentEntity = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
            if (paymentEntity == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(paymentEntity);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the payment entity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}