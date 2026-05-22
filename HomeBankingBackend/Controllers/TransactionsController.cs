using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBankingBackend.Data;
using HomeBankingBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace HomeBankingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto request)
        {
            // 1. Validaciones básicas
            if (request.Amount <= 0)
                return BadRequest("El monto a transferir debe ser mayor a cero.");

            if (request.SourceAccountId == request.DestinationAccountId)
                return BadRequest("No puedes transferir dinero a la misma cuenta.");

            // 2. Iniciamos la transacción segura de base de datos
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Buscar ambas cuentas
                var sourceAccount = await _context.Accounts.FindAsync(request.SourceAccountId);
                var destinationAccount = await _context.Accounts.FindAsync(request.DestinationAccountId);

                if (sourceAccount == null || destinationAccount == null)
                    return NotFound("Una o ambas cuentas no existen.");

                // Verificar si hay saldo suficiente
                if (sourceAccount.Balance < request.Amount)
                    return BadRequest("Fondos insuficientes en la cuenta de origen.");

                // 3. Actualizar los saldos en memoria
                sourceAccount.Balance -= request.Amount;
                destinationAccount.Balance += request.Amount;

                // 4. Crear el registro del movimiento
                var transactionRecord = new Transaction
                {
                    Amount = request.Amount,
                    Type = TransactionType.Transfer,
                    Date = DateTime.UtcNow,
                    SourceAccountId = request.SourceAccountId,
                    DestinationAccountId = request.DestinationAccountId
                };

                _context.Transactions.Add(transactionRecord);

                // 5. Guardar los cambios y confirmar la transacción
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Ok(new { message = "Transferencia exitosa", transactionId = transactionRecord.Id });
            }
            catch (Exception)
            {
                // Si algo explota, deshacemos todos los cambios para no perder dinero
                await dbTransaction.RollbackAsync();
                return StatusCode(500, "Ocurrió un error interno al procesar la transferencia.");
            }
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] AccountOperationDto request)
        {
            if (request.Amount <= 0)
                return BadRequest("El monto a depositar debe ser mayor a cero.");

            // 1. Buscamos la cuenta
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null)
                return NotFound("La cuenta no existe.");

            // 2. Sumamos la plata
            account.Balance += request.Amount;

            // 3. Creamos el comprobante
            var transactionRecord = new Transaction
            {
                Amount = request.Amount,
                Type = TransactionType.Credit, // Usamos tu Enum para ingresos
                Date = DateTime.UtcNow,
                // Como es efectivo entrando, asignamos la misma cuenta en ambos lados 
                // para evitar errores de base de datos si las columnas no permiten valores nulos.
                SourceAccountId = request.AccountId, 
                DestinationAccountId = request.AccountId 
            };

            _context.Transactions.Add(transactionRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Depósito exitoso", newBalance = account.Balance });
        }

        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] AccountOperationDto request)
        {
            if (request.Amount <= 0)
                return BadRequest("El monto a retirar debe ser mayor a cero.");

            // 1. Buscamos la cuenta
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null)
                return NotFound("La cuenta no existe.");

            // 2. Verificamos que tenga saldo suficiente
            if (account.Balance < request.Amount)
                return BadRequest("Fondos insuficientes para realizar el retiro.");

            // 3. Restamos la plata
            account.Balance -= request.Amount;

            // 4. Creamos el comprobante
            var transactionRecord = new Transaction
            {
                Amount = request.Amount,
                Type = TransactionType.Debit, // Usamos tu Enum para egresos
                Date = DateTime.UtcNow,
                SourceAccountId = request.AccountId,
                DestinationAccountId = request.AccountId 
            };

            _context.Transactions.Add(transactionRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Retiro exitoso", newBalance = account.Balance });
        }

        // --- ACÁ EMPIEZA EL NUEVO MÉTODO GET ---

        // GET: api/Transactions/Account/1
        [HttpGet("Account/{accountId}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAccountHistory(int accountId)
        {
            // Buscamos todas las transacciones donde la cuenta sea el origen O el destino
            var history = await _context.Transactions
                .Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId)
                .OrderByDescending(t => t.Date) // Ordenamos de la más reciente a la más antigua
                .ToListAsync();

            if (!history.Any())
            {
                return NotFound("No se encontraron movimientos para esta cuenta.");
            }

            return Ok(history);
        }

        // --- ACÁ TERMINA EL NUEVO MÉTODO GET ---
    }

    // Objeto para recibir los datos limpios desde internet
    public class TransferDto
    {
        public int SourceAccountId { get; set; }
        public int DestinationAccountId { get; set; }
        public decimal Amount { get; set; }
    }

    // Objeto para depósitos y retiros
    public class AccountOperationDto
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}