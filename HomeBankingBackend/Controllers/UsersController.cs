using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBankingBackend.Data;
using HomeBankingBackend.Models;

namespace HomeBankingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Ahora usamos 'Accounts' en lugar de 'Cuentas'
            return await _context.Users.Include(u => u.Accounts).ToListAsync();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); 

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }
    }
}