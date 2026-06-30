using Microsoft.EntityFrameworkCore;
using tas.Data;
using tas.Helpers;
using tas.Models;
using System.Linq;
using System.Threading.Tasks;

namespace tas.Services
{
    public class UserService
    {
        private readonly TasDbContext _context;

        public UserService(TasDbContext context) => _context = context;

        public async Task<User> AuthenticateAsync(string login, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null) return null;
            return Vertification.VerifySHA512Hash(password, user.Password) ? user : null;
        }

        public async Task<bool> RegisterAsync(string login, string password, string role = "User")
        {
            if (await _context.Users.AnyAsync(u => u.Login == login))
                return false;

            var hash = Vertification.GetSHA512Hash(password);
            var user = new User { Login = login, Password = hash, Role = role };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllUsersAsync() =>
            await _context.Users.ToListAsync();
    }
}