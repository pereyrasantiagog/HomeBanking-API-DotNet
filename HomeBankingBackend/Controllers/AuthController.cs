using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeBankingBackend.Data;
using HomeBankingBackend.Models;
using BCrypt.Net;

namespace HomeBankingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Inyectamos la base de datos y la configuración (para leer el appsettings.json)
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. Verificar si el email ya existe
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest("El email ya está registrado.");
            }

            // 2. Hashear la contraseña y crear el Usuario
            var newUser = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                // Hasheamos la contraseña antes de guardarla
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(newUser);
            
            // Guardamos para que la base de datos le asigne un Id al newUser
            await _context.SaveChangesAsync();

            // 3. Crear una cuenta bancaria automáticamente para el usuario
            var random = new Random();
            var accountNumber = $"VIN-{random.Next(10000000, 99999999)}";

            var newAccount = new Account
            {
                Number = accountNumber,
                CreationDate = DateTime.UtcNow,
                Balance = 0,
                UserId = newUser.Id
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado con éxito." });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Buscar al usuario SOLO por email en la base de datos
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Verificar que el usuario exista y que el hash coincida con la contraseña ingresada
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Email o contraseña incorrectos."); // Código 401
            }

            // 3. Crear los "Claims" (Datos del usuario que viajan seguros dentro del token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // 4. Traer la clave secreta desde appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Armar el Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // El token durará 2 horas
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 6. Devolver el Token al cliente
            return Ok(new { token = tokenString, message = "Login exitoso" });
        }
    }

    // DTO para el registro
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO para el login
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}