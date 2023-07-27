using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using truyenchu.Areas.Admin.Model;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Service;
using truyenchu.Utilities;

namespace truyenchu.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Route("Story/[action]")]
    public class StoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StoryController> _logger;
        private readonly StoryService _storyService;
        public StoryController(AppDbContext context, ILogger<StoryController> logger, StoryService storyService)
        {
            _context = context;
            _logger = logger;
            _storyService = storyService;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Story
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStories(string searchStr, int pageNumber = 1)
        {
            if (_context.Stories == null)
                return NotFound();

            var qr = _context.Stories
                                    .Include(x => x.Author)
                                    .Include(x => x.Photo)
                                    .Select(s => new
                                    {
                                        s.StoryId,
                                        s.Photo.FileName,
                                        s.Author.AuthorName,
                                        s.StoryName,
                                        s.StorySlug,
                                        s.StoryState,
                                        s.ViewCount,
                                        s.LatestChapterOrder,
                                        s.DateUpdated,
                                        s.Published
                                    })
                                    .OrderByDescending(x => x.DateUpdated)
                                    .AsQueryable();

            dynamic vm;
            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = AppUtilities.GenerateSlug(searchStr);
                // find by slug
                vm = await qr.Where(a => a.StorySlug.Contains(searchStr)).ToListAsync();

            }
            else
                vm = await qr.ToListAsync();

            var result = Pagination.PagedResults(vm, pageNumber, Const.STORIES_PER_PAGE_ADMIN);

            return Json(result);
        }

        // api 
        [HttpGet]
        public async Task<IActionResult> GetAuthor(string query)
        {
            if (_context.Stories == null)
                return NotFound();

            if (!string.IsNullOrEmpty(query))
                query = AppUtilities.GenerateSlug(query);

            var authors = await _context.Authors.Where(x => x.AuthorSlug.Contains(query)).Select(x => new { x.AuthorId, x.AuthorName }).ToListAsync();
            return Json(authors);
        }
        // GET: Story/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Stories == null)
            {
                return NotFound();
            }

            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Photo)
                .FirstOrDefaultAsync(m => m.StoryId == id);
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        // GET: Story/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.MultiSelectItem = await GetMultiSelectList();
            return View();
        }

        // POST: Story/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StoryName,AuthorId,Description,StorySource, CategoryIds")] CreateViewModel story, IFormFile? file)
        {
            ViewBag.MultiSelectItem = await GetMultiSelectList();

            if (story.AuthorId == 0)
            {
                ModelState.Remove(nameof(story.AuthorId));
                ModelState.AddModelError(nameof(story.AuthorId), "Phải chọn tác giả");
                return View(story);
            }
            if (ModelState.IsValid)
            {
                var photo = new StoryPhoto();
                photo.FileName = file != null ? UploadPhoto(file) : Const.STORY_THUMB_NO_IMAGE;
                story.Photo = photo;
                _context.StoryPhotos.Add(photo);


                story.StorySlug = GenerateStorySlug(story.StoryName);
                story.LatestChapterOrder = 0;
                story.DateCreated = DateTime.Now;
                story.DateUpdated = DateTime.Now;
                story.ViewCount = 0;
                story.StoryState = false;

                foreach (int cateId in story.CategoryIds)
                {
                    _context.Add(new StoryCategory() { CategoryId = cateId, Story = story });
                }

                _context.Add(story);
                await _context.SaveChangesAsync();

                StatusMessage = "Thêm truyện mới thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(story);
        }

        // GET: Story/Edit/5
        [Route("{id?}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || _context.Stories == null)
            {
                return NotFound();
            }

            var story = await _context.Stories.Include(x => x.StoryCategory).Include(x => x.Author).Include(x => x.Photo).FirstOrDefaultAsync(x => x.StoryId == id);
            if (story == null)
            {
                return NotFound();
            }

            var categoryIds = story.StoryCategory.Select(x => x.CategoryId).ToArray();

            var vm = new CreateViewModel(story, categoryIds);

            ViewBag.MultiSelectItem = await GetMultiSelectList();
            return View(vm);
        }

        // POST: Story/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "EditStory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm(Name = "StoryId")] int id, [Bind("StoryId,StoryName,AuthorId,Description,StorySource,StoryState,CategoryIds")] CreateViewModel model, IFormFile? file)
        {
            if (id != model.StoryId)
                return NotFound();


            var story = await _context.Stories
                                            .Include(x => x.Author)
                                            .Include(x => x.Photo)
                                            .Include(x => x.StoryCategory)
                                            .FirstOrDefaultAsync(x => x.StoryId == id);
            if (story == null)
                return NotFound();

            var categoryIds = story.StoryCategory.Select(x => x.CategoryId).ToArray();
            var vm = new CreateViewModel(story, categoryIds);
            ViewBag.MultiSelectItem = await GetMultiSelectList();
            if (model.AuthorId == 0)
            {
                ModelState.Remove(nameof(model.AuthorId));
                ModelState.AddModelError(nameof(model.AuthorId), "Phải chọn tác giả");
                return View(vm);
            }

            if (ModelState.IsValid)
            {
                try
                {

                    story.StoryName = model.StoryName;
                    story.StorySource = model.StorySource;
                    story.AuthorId = model.AuthorId;
                    story.Description = model.Description;
                    story.StoryState = model.StoryState;
                    story.DateUpdated = DateTime.Now;

                    if (file != null)
                    {
                        var photo = await _context.StoryPhotos.FindAsync(story.PhotoId);
                        // Xóa img cũ
                        if (photo != null && photo.FileName != Const.STORY_THUMB_NO_IMAGE)
                        {
                            _context.Remove(photo);
                            DeletePhoto(photo.FileName);
                        }

                        // Upload mới
                        var newPhoto = new StoryPhoto()
                        {
                            FileName = UploadPhoto(file)
                        };
                        story.Photo = newPhoto;
                        _context.StoryPhotos.Add(newPhoto);
                    }

                    var oldCateIds = story.StoryCategory.Select(x => x.CategoryId);
                    var newCateIds = model.CategoryIds;

                    var removeCates = story.StoryCategory.Where(x => !newCateIds.Contains(x.CategoryId)).ToList();
                    var addCates = newCateIds.Where(x => !oldCateIds.Contains(x)).ToList();

                    _context.StoryCategories.RemoveRange(removeCates);
                    addCates.ForEach(cate =>
                    {
                        _context.StoryCategories.Add(new StoryCategory()
                        {
                            Story = story,
                            CategoryId = cate
                        });
                    });


                    await _context.SaveChangesAsync();
                    StatusMessage = "Cập nhật truyện thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoryExists(model.StoryId))
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
            return View(vm);
        }

        [HttpPut]
        public async Task<ActionResult> UpdatePublished(int storyId, bool isPublished)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null)
                return BadRequest();

            story.Published = isPublished;
            await _context.SaveChangesAsync();
            return Ok();
        }


        // GET: Story/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Stories == null)
            {
                return NotFound();
            }

            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Photo)
                .FirstOrDefaultAsync(m => m.StoryId == id);
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        // POST: Story/Delete/5
        [HttpPost(Name = "DeleteStory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int storyId)
        {
            if (_context.Stories == null)
            {
                return Problem("Entity set 'AppDbContext.Stories'  is null.");
            }
            var story = await _context.Stories.Include(x => x.Photo).FirstOrDefaultAsync(x => x.StoryId == storyId);
            if (story != null)
            {
                if (story.Photo.FileName != Const.STORY_THUMB_NO_IMAGE)
                    DeletePhoto(story.Photo.FileName);

                _context.Stories.Remove(story);
                StatusMessage = "Xóa truyện thành công";
            }
            else
            {
                _logger.LogInformation("story null");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool StoryExists(int id)
        {
            return (_context.Stories?.Any(e => e.StoryId == id)).GetValueOrDefault();
        }

        /**
            return fileName when successful, otherwise return null
        */
        private string UploadPhoto(IFormFile file)
        {
            string imgName = null;
            if (file != null)
            {
                imgName = Path.GetFileName(Path.GetRandomFileName()) + Path.GetExtension(file.FileName);
                var uploadPath = Path.Combine(Const.STORY_THUMB_PATH, imgName);

                // resize the image
                var width = Const.STORY_THUMB_WIDTH;
                var height = Const.STORY_THUMB_HEIGHT;
                Image img = Image.FromStream(file.OpenReadStream());
                var cutImg = new Bitmap(img, width, height);

                using (var fs = new FileStream(uploadPath, FileMode.Create))
                {
                    cutImg.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            return imgName;
        }

        private void DeletePhoto(string fileName)
        {
            System.IO.File.Delete(Path.Combine(Const.STORY_THUMB_PATH, fileName));
        }

        private async Task<MultiSelectList> GetMultiSelectList()
        {
            var categories = await _context.Categories.ToListAsync();
            return new MultiSelectList(categories, nameof(Category.CategoryId), nameof(Category.CategoryName));
        }

        private string GenerateStorySlug(string name, int id = 0)
        {
            var counter = 1;
            var prefix = "";
            var slug = AppUtilities.GenerateSlug(name);
            while (_context.Stories.Any(x => x.StorySlug == slug + prefix && x.StoryId != id))
            {
                prefix = "-" + counter.ToString();
                counter++;
            }
            return slug + prefix;
        }
    }
}
