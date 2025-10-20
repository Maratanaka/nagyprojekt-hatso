using BigProject.Data;
using BigProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email és jelszó kötelező" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim());

            if (user == null || user.Password != request.Password)
                return Unauthorized(new { message = "Hibás email vagy jelszó" });

            // Sikeres login, jelszó nélkül küldjük vissza a user adatokat
            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }


        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // alapvető validáció
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email és jelszó kötelező" });

            var email = request.Email.Trim();

            // létezik-e már ilyen email?
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return Conflict(new { message = "Már létezik felhasználó ezzel az email címmel" });

            // --- Plain text tárolás (fejlesztési mód) ---
            var user = new User
            {
                Name = request.Name?.Trim() ?? "",
                Email = email,
                Password = request.Password, // ha később hash-elni szeretnéd: BCrypt.Net.BCrypt.HashPassword(request.Password)
                Role = request.Role?.Trim() ?? "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ne küldjük vissza a jelszót a válaszban
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }

        // segéd végpont, hogy CreatedAtAction működjön (opcionális)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new { u.Id, u.Name, u.Email, u.Role })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        // A korábbi Login-metódusod jöhet ide...
    }

    public class RegisterRequest
    {
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; } // opcionális
    }
}
