using Microsoft.EntityFrameworkCore;
using OAuthTraining.Models;

namespace OAuthTraining.Data
{
    /// <summary>
    /// Thin EF Core repository that exposes the persistence operations needed by the dynamic
    /// OpenID Connect configuration workflow.  Keeping the logic centralized makes it clear which
    /// queries mutate or read the single identity provider configuration record the sample uses.
    /// </summary>
    public class IdpConfigRepository
    {
        private readonly ApplicationDbContext _context;
        public IdpConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IdpConfig>> GetAllAsync()
        {
            // Exposed for completeness so the UI could list all stored configurations if needed.
            return await _context.IdpConfigs.ToListAsync();
        }

        public Task<IdpConfig?> GetCurrentAsync()
        {
            // The sample only expects at most one configuration to exist.
            return _context.IdpConfigs.SingleOrDefaultAsync();
        }

        public async Task<IdpConfig?> GetByIdAsync(int id)
        {
            return await _context.IdpConfigs.FindAsync(id);
        }

        public async Task AddAsync(IdpConfig config)
        {
            _context.IdpConfigs.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(IdpConfig config)
        {
            _context.IdpConfigs.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.IdpConfigs.FindAsync(id);
            if (config != null)
            {
                _context.IdpConfigs.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public Task<bool> HasAnyAsync()
        {
            // Used by both Program.cs (to pick a default route) and AccountController to decide if
            // the challenge should run immediately or show the configuration form.
            return _context.IdpConfigs.AnyAsync();
        }
    }
}

