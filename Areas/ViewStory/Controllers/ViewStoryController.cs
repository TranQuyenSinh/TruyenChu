using System.Diagnostics;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Utilities;

namespace truyenchu.Area.ViewStory.Controllers
{
    [Area("ViewStory")]
    public class ViewStoryController : Controller
    {
        private readonly ILogger<ViewStoryController> _logger;
        private readonly AppDbContext _dbContext;

        public ViewStoryController(ILogger<ViewStoryController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("{storySlug?}")]
        public async Task<IActionResult> DetailStory([FromRoute] string storySlug)
        {
            if (storySlug == null)
                return NotFound();
            var story = _dbContext.Stories
                        .Where(x => x.StorySlug == storySlug)
                        .Include(x => x.Author)
                        .Include(x => x.StoryCategory)
                        .ThenInclude(x => x.Category)
                        .AsQueryable()
                        .FirstOrDefault();
            if (story == null)
                return NotFound();

            var vm = new DetailViewModel();
            var chapters = await (from chapter in _dbContext.Chapters
                                  where chapter.StoryId == story.StoryId
                                  select new Chapter
                                  {
                                      Order = chapter.Order,
                                      Title = chapter.Title
                                  }).ToListAsync();

            vm.Chapters = chapters;
            vm.Story = story;
            ViewBag.Story = story;
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = story.StoryName, IsActive = true}
            };
            return View(vm);
        }

        [Route("{storySlug?}/chuong-{chapterOrder:int}")]
        public async Task<IActionResult> Chapter([FromRoute] string storySlug, [FromRoute] int chapterOrder)
        {
            var vm = new ChapterViewModel();
            var story = _dbContext.Stories.FirstOrDefault(x => x.StorySlug == storySlug);
            if (story == null)
                return NotFound();

            var chapter = _dbContext.Chapters.FirstOrDefault(x => x.StoryId == story.StoryId && x.Order == chapterOrder);
            if (chapter == null)
            {
                _logger.LogInformation("chapter null");
                return RedirectToAction(nameof(DetailStory), new { storySlug = story.StorySlug });
            }

            
            vm.Story = story;
            vm.Chapter = chapter;
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = story.StoryName ,Url = Url.Action(nameof(DetailStory), new {storySlug = story.StorySlug})},
                new BreadCrumbModel() {DisplayName = "Chương "+chapter.Order, IsActive = true}
            };

            return View(vm);
        }
        // comment
        [Route("tim-kiem/{searchString?}")]
        public async Task<IActionResult> SearchStory(string searchString)
        {
            List<SearchViewModel> model = new List<SearchViewModel>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchSlug = AppUtilities.GenerateSlug(searchString);
                var stories = await _dbContext.Stories
                                                    .Include(x => x.Author)
                                                    .Include(x => x.Photo)
                                                    .Include(x => x.StoryCategory)
                                                    .ThenInclude(x => x.Category)
                                                    .AsQueryable()
                                                    .Where(story => story.StorySlug.Contains(searchSlug) || story.Author.AuthorSlug.Contains(searchString))
                                                    .ToListAsync();
                foreach (var story in stories)
                {
                    model.Add(new SearchViewModel()
                    {
                        Photo = story.Photo,
                        StoryName = story.StoryName,
                        Author = story.Author,
                        StoryCategory = story.StoryCategory,
                        StorySlug = story.StorySlug,
                        StoryState = story.StoryState,
                        LatestChapter = _dbContext.Chapters.Where(chapter => chapter.StoryId == story.StoryId).OrderByDescending(x => x.Order).FirstOrDefault()?.Order
                    });
                }
            }

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = "Tìm truyện với từ khóa: "+searchString, IsActive = true}
            };
            ViewData["Title"] = "TÌM TRUYỆN VỚI TỪ KHOÁ: " + searchString?.ToUpper();
            return View(nameof(SearchStory), model);
        }

        [HttpGet]
        [Route("the-loai/{categorySlug}")]
        public async Task<IActionResult> SearchCategory(string categorySlug)
        {
            var category = _dbContext.Categories.FirstOrDefault(x => x.CategorySlug == categorySlug);
            if (category == null)
            {
                return NotFound();
            }

            var storiesInCate = await _dbContext.Stories.Include(x => x.Author)
                                                .Include(x => x.Photo)
                                                .Include(x => x.StoryCategory)
                                                .ThenInclude(x => x.Category)
                                                .AsQueryable()
                                                .Where(x => x.StoryCategory.Select(c => c.Category).Contains(category))
                                                .OrderBy(x => x.DateUpdated)
                                                .ToListAsync();
            List<SearchViewModel> model = new List<SearchViewModel>();
            foreach (var story in storiesInCate)
            {
                model.Add(new SearchViewModel()
                {
                    Photo = story.Photo,
                    StoryName = story.StoryName,
                    Author = story.Author,
                    StoryCategory = story.StoryCategory,
                    StoryState = story.StoryState,
                    LatestChapter = _dbContext.Chapters.Where(chapter => chapter.StoryId == story.StoryId).OrderByDescending(x => x.Order).FirstOrDefault()?.Order
                });
            }

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = category.CategoryName,  IsActive = true}
            };
            ViewBag.currentCategory = category;
            ViewData["Title"] = "TRUYỆN " + category.CategoryName.ToUpper();
            return View(nameof(SearchStory), model);
        }

        [Route("test.html")]
        public IActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}


