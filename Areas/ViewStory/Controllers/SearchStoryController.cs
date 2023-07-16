using System.Diagnostics;
using System.Text.Json.Serialization;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Service;
using truyenchu.Utilities;

namespace truyenchu.Area.ViewStory.Controllers
{
    [Area("ViewStory")]
    public class SearchStoryController : Controller
    {
        private readonly ILogger<SearchStoryController> _logger;
        private readonly AppDbContext _context;
        private readonly StoryService _storyService;

        public SearchStoryController(ILogger<SearchStoryController> logger, AppDbContext dbContext, StoryService storyService)
        {
            _logger = logger;
            _context = dbContext;
            _storyService = storyService;
        }

        [Route("tim-kiem/{searchString?}")]
        public async Task<IActionResult> SearchStory(string searchString)
        { 
            List<Story> vm = new List<Story>();
            if (!string.IsNullOrEmpty(searchString))
            {
                vm = _storyService.FindStoriesBySearchString(searchString);
            }

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = "Tìm truyện với từ khóa: "+searchString, IsActive = true}
            };
            ViewData["Title"] = "TÌM TRUYỆN VỚI TỪ KHOÁ: " + searchString?.ToUpper();
            return View(nameof(SearchStory), vm);
        }

        [HttpGet]
        [Route("the-loai/{categorySlug?}")]
        public async Task<IActionResult> SearchCategory(string? categorySlug, bool isFull = false)
        {
            List<Story> stories = new List<Story>();
            Category category = null;
            if (string.IsNullOrEmpty(categorySlug))
            {
                var qr = _context.Stories
                        .Include(x => x.Author)
                        .Include(x => x.Photo)
                        .Include(x => x.StoryCategory)
                        .ThenInclude(x => x.Category)
                        .AsQueryable();
                if (isFull)
                    qr = qr.Where(x => x.StoryState == true);
                stories = await qr.OrderByDescending(x => x.ViewCount).ToListAsync();
            }
            else
            {
                category = _context.Categories.FirstOrDefault(x => x.CategorySlug == categorySlug);
                if (category == null)
                    return NotFound();

                stories = _storyService.GetStoriesInCategory(category, isFull);
            }

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = category != null ? category.CategoryName: "Tất cả" ,
                    IsActive = true}
            };
            ViewBag.currentCategory = category;
            ViewData["Title"] = category != null ? "TRUYỆN " + category.CategoryName.ToUpper() : "THỂ LOẠI";
            return View(nameof(SearchStory), stories);
        }

        [HttpGet]
        [Route("tac-gia/{authorSlug?}")]
        public async Task<IActionResult> SearchAuthor(string authorSlug)
        {
            if (string.IsNullOrEmpty(authorSlug))
                return NotFound();
            var author = _context.Authors.FirstOrDefault(x => x.AuthorSlug.Contains(authorSlug));

            List<Story> stories = _storyService.FindStoriesByAuthor(author);

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = author.AuthorName ,
                    IsActive = true}
            };

            ViewData["Title"] = "Tác giả " + author.AuthorName;
            return View(nameof(SearchStory), stories);
        }

        [HttpGet]
        [Route("top-truyen/{rangeSlug?}")]
        public async Task<IActionResult> SearchRange(string rangeSlug)
        {
            if (string.IsNullOrEmpty(rangeSlug))
                return NotFound();
            var stories = new List<Story>();
            var breadcrumbsDisplay = "";
            switch (rangeSlug)
            {
                case "duoi-100-chuong":
                    stories = _storyService.FindStoryByRangeChapter(1, 99);
                    breadcrumbsDisplay = "Dưới 100 chương";
                    break;
                case "100-500-chuong":
                    stories = _storyService.FindStoryByRangeChapter(100, 500);
                    breadcrumbsDisplay = "100 - 500 chương";
                    break;
                case "500-1000-chuong":
                    stories = _storyService.FindStoryByRangeChapter(501, 1000);
                    breadcrumbsDisplay = "500 - 1000 chương";
                    break;
                case "tren-1000-chuong":
                    stories = _storyService.FindStoryByRangeChapter(1001);
                    breadcrumbsDisplay = "Trên 1000 chương";
                    break;
            }
            
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = breadcrumbsDisplay ,
                    IsActive = true}
            };

            ViewData["Title"] = breadcrumbsDisplay;
            return View(nameof(SearchStory), stories);
        }

        [Route("api/get-stories-by-category-slug")]
        public async Task<IActionResult> GetStoriesByCategorySlug(string type, int count, string categorySlug = "all")
        {
            List<Story> data = null;
            if (categorySlug == "all")
            {
                var qrAll = _context.Stories.Include(x => x.Photo).OrderByDescending(x => x.ViewCount).AsQueryable();
                if (type == "hot_select")
                {
                    data = await qrAll.Take(count).ToListAsync();
                    return Json(data);
                }
                else if (type == "full_select")
                {
                    data = await qrAll.Where(x => x.StoryState == true).Take(count).ToListAsync();
                    return Json(data);
                }
            }

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.CategorySlug == categorySlug);
            if (category == null)
                return BadRequest();

            var qr = _context.Stories.Include(x => x.Photo).Where(s => s.StoryCategory.Any(sc => sc.CategoryId == category.CategoryId));

            if (type == "hot_select")
            {
                data = await qr.OrderByDescending(x => x.ViewCount).Take(count).ToListAsync();
            }
            else if (type == "full_select")
            {
                data = await qr.Where(x => x.StoryState == true).OrderByDescending(x => x.ViewCount).Take(count).ToListAsync();
            }
            return Json(data);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}


