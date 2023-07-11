using System.Diagnostics;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            return View();
        }

        [Route("{storySlug}")]
        public IActionResult DetailStory([FromRoute] string storySlug)
        {
            _logger.LogInformation(storySlug);
            if (storySlug == null)
                return NotFound();
            var story = _dbContext.Stories
                        .Where(x=>x.StorySlug == storySlug)
                        .Include(x => x.StoryCategories)
                        .ThenInclude(x => x.Category)
                        .AsQueryable()
                        .FirstOrDefault();
            _logger.LogInformation(story?.Description);
            if (story == null)
                return NotFound();

            ViewBag.relatedStories = GetRelatedStories(story);
            return View(story);
        }

        [Route("{storySlug}/chuong-{chapter:int}")]
        public IActionResult Chapter([FromRoute] string storySlug, [FromRoute] int chapter)
        {
            return View();
        }
        // comment
        [Route("tim-kiem/{searchString?}")]
        public async Task<IActionResult> SearchStory(string searchString)
        {
            List<SearchViewModel> model = new List<SearchViewModel>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchSlug = AppUtilities.GenerateSlug(searchString);
                var stories = await _dbContext.Stories.Where(story => story.StorySlug.Contains(searchSlug))
                                                    .Include(x => x.Author)
                                                    .Include(x => x.Photo)
                                                    .Include(x => x.StoryCategories)
                                                    .ThenInclude(sc => sc.Category).ToListAsync();
                foreach (var story in stories)
                {
                    model.Add(new SearchViewModel()
                    {
                        Photo = story.Photo,
                        StoryName = story.StoryName,
                        Author = story.Author,
                        StoryCategories = story.StoryCategories,
                        StoryState = story.StoryState,
                        LatestChapter = _dbContext.Chapters.Where(chapter => chapter.StoryId == story.StoryId).OrderByDescending(x => x.Order).FirstOrDefault().Order
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
        public async Task<IActionResult> SearchCatgory(string categorySlug)
        {
            var category = _dbContext.Categories.FirstOrDefault(x => x.CategorySlug == categorySlug);
            if (category == null)
            {
                return NotFound();
            }

            var storiesInCate = await _dbContext.Stories.Include(x => x.Author)
                                                .Include(x => x.Photo)
                                                .Include(x => x.StoryCategories)
                                                .ThenInclude(sc => sc.Category)
                                                .AsQueryable()
                                                .Where(x => x.StoryCategories.Where(sc => sc.CategoryId == category.CategoryId).Any())
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
                    StoryCategories = story.StoryCategories,
                    StoryState = story.StoryState,
                    LatestChapter = _dbContext.Chapters.Where(chapter => chapter.StoryId == story.StoryId).OrderByDescending(x => x.Order).FirstOrDefault().Order
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

        private List<Story> GetRelatedStories(Story story)
        {
            var selectedStory = _dbContext.Stories
                                .Include(s => s.StoryCategories)
                                .ThenInclude(sc => sc.Category)
                                .Single(s => s.StoryCategories.Where(x => x.StoryId == story.StoryId).Any());

            var relatedStories = _dbContext.Stories
                                .Include(s => s.StoryCategories)
                                .ThenInclude(sc => sc.Category)
                                .Where(s => s.StoryCategories.Any(c => selectedStory.StoryCategories.Contains(c)))
                                .ToList();
            return relatedStories;
        }
    }
}


