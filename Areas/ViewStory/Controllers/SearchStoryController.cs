using System.Diagnostics;
using System.Text.Json.Serialization;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Service;
using truyenchu.Utilities;
using static truyenchu.Components.Paging;

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

        private readonly int pageSize = Const.STORIES_FOUND_PER_PAGE;


        [Route("tim-kiem/{searchString?}")]
        public async Task<IActionResult> SearchStory(string searchString, [FromQuery(Name = "trang")] int currentPage = 1)
        {
            List<Story> list = new List<Story>();
            var totalItem = 0;
            if (!string.IsNullOrEmpty(searchString))
            {
                list = _storyService.FindStoriesBySearchString(searchString);
                totalItem = list.Count;
                list = list.Skip((currentPage - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            }

            ViewBag.pagingOptions = new PagingOptions()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                totalItem = totalItem,
                GenerateUrl = (pageNum) => Url.Action(nameof(SearchStory), new
                {
                    searchString = searchString,
                    trang = pageNum
                })
            };
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = "Tìm truyện với từ khóa: "+searchString, IsActive = true}
            };
            ViewData["Title"] = "TÌM TRUYỆN VỚI TỪ KHOÁ: " + searchString?.ToUpper();
            return View(nameof(SearchStory), list);
        }

        [HttpGet]
        [Route("the-loai/{categorySlug?}")]
        public async Task<IActionResult> SearchCategory(string categorySlug, bool isFull = false, [FromQuery(Name = "trang")] int currentPage = 1)
        {
            List<Story> stories = new List<Story>();
            Category category = null;
            var totalItem = 0;

            // categorySlug = null => Find all stories
            if (string.IsNullOrEmpty(categorySlug))
            {
                var qr = _context.Stories
                        .Where(x => x.Published)
                        .Include(x => x.Author)
                        .Include(x => x.Photo)
                        .Include(x => x.StoryCategory)
                        .ThenInclude(x => x.Category)
                        .OrderByDescending(x => x.ViewCount)
                        .AsQueryable();

                if (isFull)
                    qr = qr.Where(x => x.StoryState == true);
                stories = await qr.ToListAsync();
            }
            else
            {
                category = _context.Categories.FirstOrDefault(x => x.CategorySlug == categorySlug);
                if (category == null)
                    return NotFound();

                stories = _storyService.GetStoriesInCategory(category, isFull);
            }


            totalItem = stories.Count();
            stories = stories.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.pagingOptions = new PagingOptions()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                totalItem = totalItem,
                GenerateUrl = (pageNum) => Url.Action(nameof(SearchCategory), new
                {
                    categorySlug = categorySlug,
                    trang = pageNum
                })
            };
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
        public async Task<IActionResult> SearchAuthor(string authorSlug, [FromQuery(Name = "trang")] int currentPage = 1)
        {
            if (string.IsNullOrEmpty(authorSlug))
                return NotFound();
            var author = _context.Authors.FirstOrDefault(x => x.AuthorSlug == authorSlug);

            if (author == null)
                return NotFound();

            var totalItem = 0;
            List<Story> stories = _storyService.FindStoriesByAuthor(author);
            totalItem = stories.Count();

            ViewBag.pagingOptions = new PagingOptions()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                totalItem = totalItem,
                GenerateUrl = (pageNum) => Url.Action(nameof(SearchAuthor), new
                {
                    authorSlug = authorSlug,
                    trang = pageNum
                })
            };
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = author.AuthorName ,
                    IsActive = true}
            };

            ViewData["Title"] = "Tác giả " + author.AuthorName;
            return View(nameof(SearchStory), stories.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList());
        }

        [HttpGet]
        [Route("top-truyen/{rangeSlug?}")]
        public async Task<IActionResult> SearchRange(string rangeSlug, [FromQuery(Name = "trang")] int currentPage = 1)
        {
            if (string.IsNullOrEmpty(rangeSlug))
                return NotFound();
            var stories = new List<Story>();
            var totalItem = 0;
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
            totalItem = stories.Count();
            ViewBag.pagingOptions = new PagingOptions()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                totalItem = totalItem,
                GenerateUrl = (pageNum) => Url.Action(nameof(SearchRange), new
                {
                    rangeSlug = rangeSlug,
                    trang = pageNum
                })
            };
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = breadcrumbsDisplay ,
                    IsActive = true}
            };

            ViewData["Title"] = breadcrumbsDisplay;
            return View(nameof(SearchStory), stories.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList());
        }

        [Route("api/get-stories-by-category-slug")]
        public async Task<IActionResult> GetStoriesByCategorySlug(string type, int count, string categorySlug = "all")
        {
            List<Story> data = null;
            if (categorySlug == "all")
            {
                var qrAll = _context.Stories
                            .Where(x => x.Published)
                            .Include(x => x.Photo)
                            .OrderByDescending(x => x.ViewCount)
                            .Select(x => new Story
                            {
                                StoryName = x.StoryName,
                                Photo = x.Photo,
                                StoryState = x.StoryState,
                                StorySlug = x.StorySlug,
                            })
                            .Take(count);
                if (type == "hot_select")
                    data = await qrAll.ToListAsync();
                else
                    data = await qrAll.Where(x => x.StoryState == true).ToListAsync();
                return Json(data);

            }

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.CategorySlug == categorySlug);
            if (category == null)
                return BadRequest();

            var qr = _context.Stories.Include(x => x.Photo).Where(s => s.Published && s.StoryCategory.Any(sc => sc.CategoryId == category.CategoryId));

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

        public async Task<IActionResult> SearchStoriesApi(string query)
        {
            if (!string.IsNullOrEmpty(query))
                query = AppUtilities.GenerateSlug(query);

            var stories = await _context.Stories.Where(x => x.Published && x.StorySlug.Contains(query))
                        .Select(x => new
                        {
                            x.StoryName,
                            x.StorySlug
                        }).ToListAsync();
            return Json(stories);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}


