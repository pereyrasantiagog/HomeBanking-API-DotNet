using HomeBankingBackend.Data;
using HomeBankingBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HomeBankingBackend.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> TransferAsync(int loggedInUserId, int sourceAccountId, int destinationAccountId, decimal amount)
        {
            if (amount <= 0)
                return new ServiceResult { Success = false, StatusCode = 400, Message = "El monto a transferir debe ser mayor a cero." };

            if (sourceAccountId == destinationAccountId)
                return new ServiceResult { Success = false, StatusCode = 400, Message = "No puedes transferir dinero a la misma cuenta." };

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sourceAccount = await _context.Accounts.FindAsync(sourceAccountId);
                var destinationAccount = await _context.Accounts.FindAsync(destinationAccountId);

                if (sourceAccount == null || destinationAccount == null)
                    return new ServiceResult { Success = false, StatusCode = 404, Message = "Una o ambas cuentas no existen." };

                if (sourceAccount.UserId != loggedInUserId)
                    return new ServiceResult { Success = false, StatusCode = 403, Message = "No tienes permiso para operar esta cuenta." };

                if (sourceAccount.Balance < amount)
                    return new ServiceResult { Success = false, StatusCode = 400, Message = "Fondos insuficientes en la cuenta de origen." };

                sourceAccount.Balance -= amount;
                destinationAccount.Balance += amount;

                var transactionRecord = new Transaction
                {
                    Amount = amount,
                    Type = TransactionType.Transfer,
                    Date = DateTime.UtcNow,
                    SourceAccountId = sourceAccountId,
                    DestinationAccountId = destinationAccountId
                };

                _context.Transactions.Add(transactionRecord);

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new ServiceResult 
                { 
                    Success = true, 
                    StatusCode = 200, 
                    Message = "Transferencia exitosa", 
                    Data = new { transactionId = transactionRecord.Id } 
                };
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                return new ServiceResult { Success = false, StatusCode = 500, Message = "Ocurrió un error interno al procesar la transferencia." };
            }
        }

        public async Task<ServiceResult> DepositAsync(int loggedInUserId, int accountId, decimal amount)
        {
            if (amount <= 0)
                return new ServiceResult { Success = false, StatusCode = 400, Message = "El monto a depositar debe ser mayor a cero." };

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return new ServiceResult { Success = false, StatusCode = 404, Message = "La cuenta no existe." };

            if (account.UserId != loggedInUserId)
                return new ServiceResult { Success = false, StatusCode = 403, Message = "No tienes permiso para operar esta cuenta." };

            account.Balance += amount;

            var transactionRecord = new Transaction
            {
                Amount = amount,
                Type = TransactionType.Credit,
                Date = DateTime.UtcNow,
                SourceAccountId = accountId,
                DestinationAccountId = accountId 
            };

            _context.Transactions.Add(transactionRecord);
            await _context.SaveChangesAsync();

            return new ServiceResult 
            { 
                Success = true, 
                StatusCode = 200, 
                Message = "Depósito exitoso", 
                Data = new { newBalance = account.Balance } 
            };
        }

        public async Task<ServiceResult> WithdrawAsync(int loggedInUserId, int accountId, decimal amount)
        {
            if (amount <= 0)
                return new ServiceResult { Success = false, StatusCode = 400, Message = "El monto a retirar debe ser mayor a cero." };

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return new ServiceResult { Success = false, StatusCode = 404, Message = "La cuenta no existe." };

            if (account.UserId != loggedInUserId)
                return new ServiceResult { Success = false, StatusCode = 403, Message = "No tienes permiso para operar esta cuenta." };

            if (account.Balance < amount)
                return new ServiceResult { Success = false, StatusCode = 400, Message = "Fondos insuficientes para realizar el retiro." };

            account.Balance -= amount;

            var transactionRecord = new Transaction
            {
                Amount = amount,
                Type = TransactionType.Debit,
                Date = DateTime.UtcNow,
                SourceAccountId = accountId,
                DestinationAccountId = accountId 
            };

            _context.Transactions.Add(transactionRecord);
            await _context.SaveChangesAsync();

            return new ServiceResult 
            { 
                Success = true, 
                StatusCode = 200, 
                Message = "Retiro exitoso", 
                Data = new { newBalance = account.Balance } 
            };
        }

        public async Task<ServiceResult> GetAccountHistoryAsync(int loggedInUserId, int accountId, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return new ServiceResult { Success = false, StatusCode = 404, Message = "La cuenta no existe." };
            
            if (account.UserId != loggedInUserId)
                return new ServiceResult { Success = false, StatusCode = 403, Message = "No tienes permiso para ver esta cuenta." };

            var query = _context.Transactions
                .Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId)
                .OrderByDescending(t => t.Date);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var history = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!history.Any() && pageNumber == 1)
                return new ServiceResult { Success = false, StatusCode = 404, Message = "No se encontraron movimientos para esta cuenta." };

            return new ServiceResult 
            { 
                Success = true, 
                StatusCode = 200, 
                Data = new 
                {
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Transactions = history
                }
            };
        }
    }
}