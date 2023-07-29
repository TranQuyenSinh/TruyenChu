using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using truyenchu.Components;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Utilities;

namespace truyenchu.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Route("manage-author/[action]")]
    public class AuthorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(AppDbContext context, ILogger<AuthorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Author
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthors(string searchStr, int pageNumber = 1)
        {
            if (_context.Authors == null)
                return NotFound();

            List<Author> authors = new List<Author>();
            var qr = _context.Authors.OrderByDescending(x => x.DateUpdated);

            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = AppUtilities.GenerateSlug(searchStr);
                // find by slug
                authors = await qr.Where(a => a.AuthorSlug.Contains(searchStr))
                                    .ToListAsync();
            }
            else
                authors = await qr.ToListAsync();

            var result = Pagination.PagedResults(authors, pageNumber, Const.AUTHORS_FOUND_PER_PAGE);

            return Json(result);
        }

        // GET: Author/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Author/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorName")] Author author)
        {
            ModelState.Remove(nameof(author.AuthorSlug));
            if (!ModelState.IsValid)
                return View();

            author.AuthorSlug = GenerateAuthorSlug(author);
            author.DateCreated = DateTime.Now;
            author.DateUpdated = DateTime.Now;

            _context.Add(author);
            await _context.SaveChangesAsync();
            StatusMessage = "Thêm tác giả thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Author/Edit/5
        [Route("{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Authors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Author/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "EditAuthor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm(Name = "AuthorId")] int id, [Bind("AuthorId,AuthorName")] Author author)
        {
            if (id != author.AuthorId) return NotFound();

            ModelState.Remove(nameof(author.AuthorSlug));
            if (!ModelState.IsValid) return View(author);

            author.AuthorSlug = GenerateAuthorSlug(author);
            author.DateUpdated = DateTime.Now;

            try
            {
                _context.Update(author);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(author.AuthorId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            StatusMessage = "Cập nhật tác giả thành công";
            return RedirectToAction(nameof(Index));

        }

        [Route("{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Authors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.AuthorId == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Author/Delete/5
        [HttpPost(Name = "DeleteAuthor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int authorId)
        {
            if (_context.Authors == null)
            {
                return Problem("Entity set 'AppDbContext.Authors'  is null.");
            }
            var author = await _context.Authors.FindAsync(authorId);
            if (author != null)
            {
                _context.Authors.Remove(author);
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Xóa tác giả thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return (_context.Authors?.Any(e => e.AuthorId == id)).GetValueOrDefault();
        }

        // abc-xyz ==> abc-xyz1, abc-xyz2,...
        private string GenerateAuthorSlug(Author author)
        {
            var counter = 1;
            var prefix = "";
            var slug = AppUtilities.GenerateSlug(author.AuthorName);
            while (_context.Authors.Any(x => x.AuthorSlug == slug + prefix && x.AuthorId != author.AuthorId))
            {
                prefix = "-" + counter.ToString();
                counter++;
            }
            return slug + prefix;
        }
    }
}
