using Microsoft.AspNetCore.Mvc;
using HomeBankingBackend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HomeBankingBackend.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeBankingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int loggedInUserId))
            {
                return loggedInUserId;
            }
            return 0; // Return 0 or handle invalid token appropriately
        }

        private IActionResult HandleResult(ServiceResult result)
        {
            if (result.Success)
            {
                return Ok(result.Data ?? new { message = result.Message });
            }

            return result.StatusCode switch
            {
                400 => BadRequest(result.Message),
                403 => Forbid(),
                404 => NotFound(result.Message),
                _ => StatusCode(500, result.Message)
            };
        }

        [HttpPost("Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto request)
        {
            int loggedInUserId = GetLoggedInUserId();
            if (loggedInUserId == 0) return Unauthorized("Token inválido o malformado.");

            var result = await _transactionService.TransferAsync(
                loggedInUserId, request.SourceAccountId, request.DestinationAccountId, request.Amount);

            return HandleResult(result);
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] AccountOperationDto request)
        {
            int loggedInUserId = GetLoggedInUserId();
            if (loggedInUserId == 0) return Unauthorized("Token inválido o malformado.");

            var result = await _transactionService.DepositAsync(
                loggedInUserId, request.AccountId, request.Amount);

            return HandleResult(result);
        }

        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] AccountOperationDto request)
        {
            int loggedInUserId = GetLoggedInUserId();
            if (loggedInUserId == 0) return Unauthorized("Token inválido o malformado.");

            var result = await _transactionService.WithdrawAsync(
                loggedInUserId, request.AccountId, request.Amount);

            return HandleResult(result);
        }

        [HttpGet("History/{accountId}")]
        public async Task<IActionResult> GetAccountHistory(int accountId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            int loggedInUserId = GetLoggedInUserId();
            if (loggedInUserId == 0) return Unauthorized("Token inválido o malformado.");

            var result = await _transactionService.GetAccountHistoryAsync(loggedInUserId, accountId, pageNumber, pageSize);

            return HandleResult(result);
        }
    }

    // Objeto para recibir los datos limpios desde internet
    public class TransferDto
    {
        [Required(ErrorMessage = "La cuenta de origen es obligatoria.")]
        public int SourceAccountId { get; set; }

        [Required(ErrorMessage = "La cuenta de destino es obligatoria.")]
        public int DestinationAccountId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a transferir debe ser mayor a cero.")]
        public decimal Amount { get; set; }
    }

    // Objeto para depósitos y retiros
    public class AccountOperationDto
    {
        [Required(ErrorMessage = "El ID de la cuenta es obligatorio.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la operación debe ser mayor a cero.")]
        public decimal Amount { get; set; }
    }
}