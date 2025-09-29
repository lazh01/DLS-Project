using DraftService.Data;
using DraftService.Models;
using DraftService.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Services
{
    public class DraftModelService
    {
        private readonly DraftServiceContext _context;

        // DbContext will be injected here
        public DraftModelService(DraftServiceContext context)
        {
            _context = context;
        }

        public async Task<Draft?> GetDraftByIdAsync(int id)
        {
            return await _context.Draft.FindAsync(id);
        }

        public async Task<List<Draft>> GetDraftsByAuthorAsync(string author)
        {
            return await _context.Draft
                .Where(d => d.Author == author)
                .ToListAsync();
        }

        public async Task<Draft> CreateDraftAsync(CreateDraftRequest request)
        {
            var draft = new Draft
            {
                Title = request.Title,
                Content = request.Content,
                Author = request.Author,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Draft.Add(draft);
            await _context.SaveChangesAsync();
            return draft;
        }

        public async Task<Draft?> UpdateDraftAsync(int id, UpdateDraftRequest request)
        {
            var draft = await _context.Draft.FindAsync(id);
            if (draft == null)
            {
                return null;
            }
            draft.Title = request.Title ?? draft.Title;
            draft.Content = request.Content ?? draft.Content;
            draft.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return draft;
        }
    }
}
