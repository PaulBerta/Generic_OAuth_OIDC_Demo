using Microsoft.EntityFrameworkCore;
using OAuthTraining.Models;

namespace OAuthTraining.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<IdpConfig> IdpConfigs { get; set; }
    }
}

