using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DraftService.Data;
using DraftService.Models;
using DraftService.Services;
using DraftService.SharedModels;

namespace DraftService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DraftsTestController : Controller
    {
        private readonly DraftServiceContext _context;
        private readonly DraftModelService _service;

        public DraftsTestController(DraftServiceContext context, DraftModelService service)
        {
            _context = context;
            _service = service;
        }

        // GET: Drafts
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Draft.ToListAsync());
        }

        // GET: Drafts/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var draft = await _context.Draft
                .FirstOrDefaultAsync(m => m.Id == id);
            if (draft == null)
            {
                return NotFound();
            }

            return View(draft);
        }

        // GET: Drafts/Create
        /*
        public IActionResult Create()
        {
            return View();
        }
        */

        // POST: Drafts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(Draft draft)
        {
            if (ModelState.IsValid)
            {
                _context.Add(draft);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(draft);
        }

        // GET: Drafts/Edit/5
        /*
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var draft = await _context.Draft.FindAsync(id);
            if (draft == null)
            {
                return NotFound();
            }
            return View(draft);
        }*/

        // POST: Drafts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut("{Id}")]
        public async Task<IActionResult> Edit(int id, Draft draft)
        {
            if (id != draft.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(draft);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DraftExists(draft.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(draft);
        }

        // GET: Drafts/Delete/5
        /*
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var draft = await _context.Draft
                .FirstOrDefaultAsync(m => m.Id == id);
            if (draft == null)
            {
                return NotFound();
            }

            return View(draft);
        }*/

        // POST: Drafts/Delete/5
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var draft = await _context.Draft.FindAsync(id);
            if (draft != null)
            {
                _context.Draft.Remove(draft);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        private bool DraftExists(int id)
        {
            return _context.Draft.Any(e => e.Id == id);
        }
        
    }
}
