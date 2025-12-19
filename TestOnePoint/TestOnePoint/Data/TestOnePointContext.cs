using Microsoft.EntityFrameworkCore;
using TestOnePoint.Models;

namespace TestOnePoint.Data
{
    public class TestOnePointContext : DbContext
    {
        public TestOnePointContext(DbContextOptions<TestOnePointContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;
    }
}