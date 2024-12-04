using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserRegistrationSystem.Data;
using UserRegistrationSystem.Models;

namespace UserRegistrationSystem.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            try
            {
                var email = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (email)
                {
                    return false;
                }
                user.PasswordHash = HashPassword(password);
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return false;
            }
            
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && VerifyPassword(user.PasswordHash, password))
            {
                return user;
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication: {ex.Message}");
            }
            return null;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        private bool VerifyPassword(string hash, string password)
        {
            string hashedPassword = HashPassword(password);
            return hash == hashedPassword;
        }

        public async Task<List<User>> GetUsersAsync(int page, int pageSize)
        {
            try
            {
                return await _context.Users
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<int> GetUsersCountAsync()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user count: {ex.Message}");
                return 0;
            }
        }

    }
}
