using Microsoft.EntityFrameworkCore;
using BigProject.Models;
using System.Collections.Generic;

namespace BigProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}
