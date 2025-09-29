using DraftService.Data;
using DraftService.Services;
using DraftService.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DraftService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DraftsController : ControllerBase  {
        private readonly DraftModelService _service;

        public DraftsController(DraftServiceContext context, DraftModelService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetDraftById(int id)
        {
            var draft = await _service.GetDraftByIdAsync(id);
            if (draft == null)
            {
                return NotFound();
            }
            return Ok(draft);
        }
        [HttpGet("author/{author}")]
        public async Task<IActionResult> GetDraftsByAuthor(string author)
        {
            var drafts = await _service.GetDraftsByAuthorAsync(author);
            return Ok(drafts);
        }
        [HttpPost]
        public async Task<IActionResult> CreateDraft(CreateDraftRequest request)
        {
            var draft = await _service.CreateDraftAsync(request);
            return CreatedAtAction(nameof(GetDraftById), new { id = draft.Id }, draft);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDraft(int id, UpdateDraftRequest request)
        {
            var draft = await _service.UpdateDraftAsync(id, request);
            if (draft == null)
            {
                return NotFound();
            }
            return Ok(draft);
        }
    }
}
