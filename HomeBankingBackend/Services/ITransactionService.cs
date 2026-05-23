using System.Threading.Tasks;

namespace HomeBankingBackend.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public int StatusCode { get; set; }
    }

    public interface ITransactionService
    {
        Task<ServiceResult> TransferAsync(int loggedInUserId, int sourceAccountId, int destinationAccountId, decimal amount);
        Task<ServiceResult> DepositAsync(int loggedInUserId, int accountId, decimal amount);
        Task<ServiceResult> WithdrawAsync(int loggedInUserId, int accountId, decimal amount);
        Task<ServiceResult> GetAccountHistoryAsync(int loggedInUserId, int accountId, int pageNumber = 1, int pageSize = 10);
    }
}