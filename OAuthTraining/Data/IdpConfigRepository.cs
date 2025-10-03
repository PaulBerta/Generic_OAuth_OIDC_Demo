using Microsoft.EntityFrameworkCore;
using OAuthTraining.Models;

namespace OAuthTraining.Data
{
    public class IdpConfigRepository
    {
        private readonly ApplicationDbContext _context;
        public IdpConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IdpConfig>> GetAllAsync()
        {
            return await _context.IdpConfigs.ToListAsync();
        }

        public Task<IdpConfig?> GetCurrentAsync()
        {
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
            return _context.IdpConfigs.AnyAsync();
        }
    }
}

