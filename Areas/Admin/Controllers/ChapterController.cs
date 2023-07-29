using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Utilities;

namespace truyenchu.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Route("manage-chapter/[action]")]
    public class ChapterController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ChapterController> _logger;

        public ChapterController(AppDbContext context, ILogger<ChapterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        // GET: Chapter
        [HttpGet]
        [Route("{storyId?}")]
        public async Task<IActionResult> Index(int storyId)
        {
            var story = await _context.Stories
                                            .Include(x => x.Author)
                                            .Include(x => x.Photo)
                                            .Include(x => x.StoryCategory)
                                            .ThenInclude(sc => sc.Category)
                                            .FirstOrDefaultAsync(x => x.StoryId == storyId);
            if (story == null)
                return NotFound();

            return View(story);
        }

        [HttpGet]
        public async Task<IActionResult> GetChapters(int storyId, int pageNumber = 1)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null)
            {
                return NotFound();
            }

            var chapters = await _context.Chapters.Where(x => x.StoryId == story.StoryId)
                            .Select(x => new Chapter { ChapterId = x.ChapterId, Title = x.Title, Order = x.Order, DateCreated = x.DateCreated })
                            .OrderByDescending(x => x.Order)
                            .ToListAsync();
            var pageSize = Const.CHAPTERS_PER_PAGE_ADMIN;
            Pagination.PagingData<Chapter> pagedData = Pagination.PagedResults(chapters, pageNumber, pageSize);
            return Json(pagedData);
        }

        // GET: Chapter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Chapters == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.ChapterId == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // GET: Chapter/Create
        [HttpGet]
        [Route("{storyId?}")]
        public async Task<IActionResult> Create(int storyId)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null)
                return NotFound();
            ViewBag.Story = story;
            var chapter = TempData["CreateChapterModel"] != null ? JsonConvert.DeserializeObject<Chapter>(TempData["CreateChapterModel"] as string) : null;

            return View(chapter);
        }

        // POST: Chapter/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "CreateChapter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,StoryId")] Chapter chapter, int storyId)
        {
            var canCreate = true;
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(chapter.Content))
            {
                ErrorMessage = "Nội dung chương không được để trống";
                canCreate = false;
            }


            if (canCreate && ModelState.IsValid)
            {
                chapter.DateCreated = DateTime.Now;
                chapter.Order = story.LatestChapterOrder + 1;
                chapter.StoryId = story.StoryId;

                story.DateUpdated = DateTime.Now;
                story.LatestChapterOrder = chapter.Order;

                _context.Add(chapter);
                await _context.SaveChangesAsync();

                StatusMessage = "Thêm chương mới thành công";
                return RedirectToAction(nameof(Index), new { storyId });
            }

            TempData["CreateChapterModel"] = JsonConvert.SerializeObject(chapter);
            return RedirectToAction(nameof(Create), new { storyId });
        }

        // GET: Chapter/Edit/5
        [Route("{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Chapters == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.Include(x => x.Story).FirstOrDefaultAsync(x => x.ChapterId == id);
            if (chapter == null)
            {
                return NotFound();
            }
            return View(chapter);
        }

        // POST: Chapter/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "EditChapter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm(Name = "ChapterId")] int id, [Bind("ChapterId,Title,Content,StoryId, Order, DateCreated")] Chapter model, int oldOrder)
        {
            if (id != model.ChapterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // update chapter order
                var index = oldOrder;
                var des = model.Order;
                // giảm order 5 -> 1 => correct
                if (des < index)
                {
                    _context.Chapters.Where(x => x.StoryId == model.StoryId && x.Order >= des && x.Order < index)
                                    .ToList()
                                    .ForEach(c => c.Order++);
                }
                else if (des > index) // 1 => 5
                {
                    _context.Chapters.Where(x => x.StoryId == model.StoryId && x.Order > index && x.Order <= des)
                                    .ToList()
                                    .ForEach(c => c.Order--);
                }

                try
                {
                    _context.Update(model);

                    StatusMessage = "Cập nhật chương thành công";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(model.ChapterId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { storyId = model.StoryId });
            }
            ViewData["StoryId"] = new SelectList(_context.Stories, "StoryId", "Description", model.StoryId);
            return View(model);
        }

        // GET: Chapter/Delete/5
        [Route("{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Chapters == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.ChapterId == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Chapter/Delete/5
        [HttpPost(Name = "DeleteChapter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm(Name = "ChapterId")] int id)
        {
            if (_context.Chapters == null)
            {
                return Problem("Entity set 'AppDbContext.Chapters'  is null.");
            }
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return NotFound();

            var story = await _context.Stories.FindAsync(chapter.StoryId);
            if (story == null) return NotFound();

            _context.Chapters.Where(x => x.StoryId == story.StoryId && x.Order > chapter.Order)
                            .ToList().ForEach(c => c.Order--);
            _context.Remove(chapter);
            story.LatestChapterOrder--;
            await _context.SaveChangesAsync();

            StatusMessage = "Xóa chương thành công";
            return RedirectToAction(nameof(Index), new { storyId = story.StoryId });
        }

        private bool ChapterExists(int id)
        {
            return (_context.Chapters?.Any(e => e.ChapterId == id)).GetValueOrDefault();
        }
    }
}
