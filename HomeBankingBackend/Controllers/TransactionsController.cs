using Microsoft.AspNetCore.Mvc;
using HomeBankingBackend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HomeBankingBackend.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HomeBankingBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeBankingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly AppDbContext _context;

        public TransactionsController(ITransactionService transactionService, AppDbContext context)
        {
            _transactionService = transactionService;
            _context = context;
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

        [HttpGet("History")]
        public async Task<IActionResult> GetAccountHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            int loggedInUserId = GetLoggedInUserId();
            if (loggedInUserId == 0) return Unauthorized("Token inválido o malformado.");

            var userAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == loggedInUserId);
            if (userAccount == null) return NotFound("No se encontró una cuenta para este usuario.");

            var result = await _transactionService.GetAccountHistoryAsync(loggedInUserId, userAccount.Id, pageNumber, pageSize);

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

    public class DepositDto
    {
        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto a depositar debe ser mayor a cero.")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}