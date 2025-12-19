using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestOnePoint.Models;

namespace TestOnePoint.Data
{
    public class TestOnePointContext(DbContextOptions<TestOnePointContext> options) : DbContext(options)
    { 

        public DbSet<TestOnePoint.Models.User> Users { get; set; } = default!;
    }
}
