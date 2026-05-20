using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBankingBackend.Data;
using HomeBankingBackend.Models;

namespace HomeBankingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Accounts
        // Trae todas las cuentas existentes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }

        // POST: api/Accounts
        // Crea una cuenta nueva y la asocia a un usuario
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(Account account)
        {
            // 1. Validamos que el UsuarioId que nos envían realmente exista en la base de datos
            var userExists = await _context.Users.AnyAsync(u => u.Id == account.UserId);
            if (!userExists)
            {
                return BadRequest("Error: El usuario especificado no existe.");
            }

            // 2. Si el usuario existe, guardamos la cuenta
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccounts), new { id = account.Id }, account);
        }
    }
}