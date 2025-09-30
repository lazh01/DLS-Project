using DraftService.Data;
using DraftService.Models;
using DraftService.SharedModels;
using Monitoring;
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
            using (var activity = MonitorService.ActivitySource.StartActivity())
            {
                if (activity == null)
                {
                    MonitorService.Log.Debug("no activity");
                }
                try
                {
                    return await _context.Draft.FindAsync(id);
                }
                catch (Exception ex)
                {
                    MonitorService.Log.Error(messageTemplate: "Error fetching draft with ID: {Id} - {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
        }

        public async Task<List<Draft>> GetDraftsByAuthorAsync(string author)
        {
            MonitorService.Log.Information(messageTemplate : "Fetching drafts for author: {Author}", author);
            try
            {
                return await _context.Draft
                    .Where(d => d.Author == author)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MonitorService.Log.Error(messageTemplate : "Error fetching drafts for author: {Author} - {ErrorMessage}", author, ex.Message);
                throw;
            }
        }


        public async Task<Draft> CreateDraftAsync(CreateDraftRequest request)
        {
            using (var activity = MonitorService.ActivitySource.StartActivity())
            {
                MonitorService.Log.Information(messageTemplate : "Creating new draft with title: {Title} for author: {Author}", request.Title, request.Author);
                try
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
                catch (DbUpdateException ex)
                {
                    MonitorService.Log.Error(messageTemplate : "Error creating draft with title: {Title} for author: {Author} - {ErrorMessage}", request.Title, request.Author, ex.Message);
                    throw;
                }
            }
        }

        public async Task<Draft?> UpdateDraftAsync(int id, UpdateDraftRequest request)
        {
            MonitorService.Log.Information(messageTemplate : "Updating draft with ID: {Id}", id);
            try
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
            catch (DbUpdateException ex)
            {
                MonitorService.Log.Error(messageTemplate : "Error updating draft with ID: {Id} - {ErrorMessage}", id, ex.Message);
                throw;
            }
        }
    }
}
