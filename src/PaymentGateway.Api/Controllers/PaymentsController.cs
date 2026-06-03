using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Payments;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Api.Controllers;

/// <summary>Payment processing and retrieval.</summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Process a card payment through the acquiring bank.</summary>
    /// <remarks>
    /// The card number is never stored in full. Only the last four digits are retained.
    /// CVV is used solely for bank authorization and is never persisted.
    ///
    /// Possible outcomes:
    /// - **Authorized** — bank approved the payment.
    /// - **Declined** — bank declined the payment.
    /// - **Rejected** — request was invalid; the bank was not called.
    /// </remarks>
    /// <response code="200">Payment was authorized or declined by the acquiring bank.</response>
    /// <response code="422">Request contained invalid data. The acquiring bank was not called.</response>
    /// <response code="502">The acquiring bank is currently unavailable.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        try
        {
            var result = await _paymentService.ProcessAsync(new ProcessPaymentRequest(
                request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.Currency,
                request.Amount,
                request.Cvv));

            if (result.Status == PaymentStatus.Rejected)
                return UnprocessableEntity(new { status = nameof(PaymentStatus.Rejected) });

            return Ok(ToResponse(result));
        }
        catch (BankUnavailableException)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "The acquiring bank is currently unavailable. Please try again later." });
        }
    }

    /// <summary>Retrieve a previously processed payment by its identifier.</summary>
    /// <param name="id">The unique payment identifier returned when the payment was processed.</param>
    /// <response code="200">Payment found — returns masked card details and status.</response>
    /// <response code="404">No payment exists with the given identifier.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostPaymentResponse>> GetPaymentAsync(Guid id)
    {
        var result = await _paymentService.GetAsync(id);

        if (result is null)
            return NotFound();

        return Ok(ToResponse(result));
    }

    private static PostPaymentResponse ToResponse(PaymentResult r) => new()
    {
        Id = r.Id,
        Status = r.Status,
        CardNumberLastFour = r.CardNumberLastFour!,
        ExpiryMonth = r.ExpiryMonth,
        ExpiryYear = r.ExpiryYear,
        Currency = r.Currency!,
        Amount = r.Amount
    };
}
