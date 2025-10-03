using Microsoft.EntityFrameworkCore;
using OAuthTraining.Models;

namespace OAuthTraining.Data
{
    /// <summary>
    /// Entity Framework Core context that stores the identity provider configuration supplied by
    /// the operator through the UI.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// Table containing the single identity provider configuration record used by the sample.
        /// </summary>
        public DbSet<IdpConfig> IdpConfigs => Set<IdpConfig>();
    }
}

