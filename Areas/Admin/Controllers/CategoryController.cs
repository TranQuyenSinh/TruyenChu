using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Utilities;

namespace truyenchu.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Route("Category/[action]")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Category
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(string searchStr, int pageNumber = 1)
        {
            if (_context.Categories == null)
                return NotFound();

            List<Category> categories = new List<Category>();
            var qr = _context.Categories.OrderByDescending(x => x.CategoryId);

            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = AppUtilities.GenerateSlug(searchStr);
                // find by slug
                categories = await qr.Where(a => a.CategorySlug.Contains(searchStr))
                                    .ToListAsync();
            }
            else
                categories = await qr.ToListAsync();

            var result = Pagination.PagedResults(categories, pageNumber, Const.CATEGORIES_FOUND_PER_PAGE);

            return Json(result);
        }

        // GET: Category/Details/5
        [HttpGet]
        [Route("{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,Description")] Category category)
        {
            ModelState.Remove(nameof(Category.CategorySlug));
            if (!ModelState.IsValid)
                return View();

            category.CategorySlug = GenerateCategorySlug(category);

            _context.Add(category);
            await _context.SaveChangesAsync();
            StatusMessage = "Thêm thể loại thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "EditCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm(Name = "CategoryId")] int id, [Bind("CategoryId,CategoryName,Description")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(category.CategorySlug));
            if (!ModelState.IsValid) return View(category);

            category.CategorySlug = GenerateCategorySlug(category);

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.CategoryId))
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

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost(Name = "DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm(Name = "CategoryId")] int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thể loại thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }

        // abc-xyz ==> abc-xyz1, abc-xyz2,...
        private string GenerateCategorySlug(Category cate)
        {
            var counter = 1;
            var prefix = "";
            var slug = AppUtilities.GenerateSlug(cate.CategoryName);
            while (_context.Categories.Any(x => x.CategorySlug == slug + prefix && x.CategoryId != cate.CategoryId))
            {
                prefix = "-" + counter.ToString();
                counter++;
            }
            return slug + prefix;
        }
    }
}
