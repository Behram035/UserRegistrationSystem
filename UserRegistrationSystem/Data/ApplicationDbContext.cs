using Microsoft.EntityFrameworkCore;
using System.Data;
using UserRegistrationSystem.Models;

namespace UserRegistrationSystem.Data
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

    }
}
