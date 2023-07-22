using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Components;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Service;
using truyenchu.Utilities;

namespace truyenchu.Area.ViewStory.Controllers
{
    [Area("ViewStory")]
    public class ViewStoryController : Controller
    {
        private readonly ILogger<ViewStoryController> _logger;
        private readonly AppDbContext _context;
        private readonly StoryService _storyService;

        public ViewStoryController(ILogger<ViewStoryController> logger, AppDbContext dbContext, StoryService storyService)
        {
            _logger = logger;
            _context = dbContext;
            _storyService = storyService;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var vm = new IndexViewModel();
            var cookie = Request.Cookies[Const.READING_STORY_COOKIE_NAME];
            if (cookie != null)
            {
                var list = JsonConvert.DeserializeObject<List<ReadingStory>>(cookie);
                vm.ReadingStories = list.OrderByDescending(x => x.LatestReading).ToList();
            }
            var categories = await _context.Categories.ToListAsync();
            vm.SelectListItems = new SelectList(categories, nameof(Category.CategorySlug), nameof(Category.CategoryName));
            return View(vm);
        }

        [Route("{storySlug?}")]
        public async Task<IActionResult> DetailStory([FromRoute] string storySlug)
        {
            if (storySlug == null)
                return NotFound();
            var story = _context.Stories
                        .Where(x => x.StorySlug == storySlug)
                        .Include(x => x.Author)
                        .Include(x => x.Photo)
                        .Include(x => x.StoryCategory)
                        .ThenInclude(x => x.Category)
                        .AsQueryable()
                        .FirstOrDefault();
            if (story == null)
                return NotFound();

            var vm = new DetailViewModel();
            var chapters = await (from chapter in _context.Chapters
                                  where chapter.StoryId == story.StoryId
                                  orderby chapter.Order
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

        [Route("{storySlug?}/chuong-{chapterOrder}")]
        public async Task<IActionResult> Chapter(string storySlug, int chapterOrder)
        {
            var vm = new ChapterViewModel();
            var story = _context.Stories.FirstOrDefault(x => x.StorySlug == storySlug);
            if (story == null)
                return NotFound();

            var chapter = _context.Chapters.FirstOrDefault(x => x.StoryId == story.StoryId && x.Order == chapterOrder);
            if (chapter == null)
            {
                return RedirectToAction(nameof(DetailStory), new { storySlug = story.StorySlug });
            }

            // save current reading story into cookie
            var json = GenerateCookieJson(story, chapter);
            Response.Cookies.Append(Const.READING_STORY_COOKIE_NAME, json);

            vm.Story = story;
            vm.Chapter = chapter;
            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {DisplayName = story.StoryName ,Url = Url.Action(nameof(DetailStory), new {storySlug = story.StorySlug})},
                new BreadCrumbModel() {DisplayName = "Chương "+chapter.Order, IsActive = true}
            };

            return View(vm);
        }

        [Route("api/get-chapter")]
        [HttpGet]
        public async Task<IActionResult> GetChapterAPI(string storySlug, int pageNumber = 1)
        {
            var story = await _context.Stories.FirstOrDefaultAsync(x => x.StorySlug == storySlug);
            if (story == null)
                return BadRequest();

            var chapters = await _context.Chapters.Where(x => x.StoryId == story.StoryId)
                            .Select(x => new Chapter { Order = x.Order, Title = x.Title })
                            .OrderBy(x => x.Order)
                            .ToListAsync();
            var pageSize = Const.CHAPTER_PER_PAGE;
            Pagination.PagingData<Chapter> pagedData = Pagination.PagedResults(chapters, pageNumber, pageSize);
            return Json(pagedData);

        }

        private string GenerateCookieJson(Story story, Chapter chapter)
        {
            var cookieItem = new ReadingStory()
            {
                StoryName = story.StoryName,
                StorySlug = story.StorySlug,
                ChapterOrder = chapter.Order,
                LatestReading = DateTime.Now
            };

            var cookie = Request.Cookies[Const.READING_STORY_COOKIE_NAME];
            if (cookie == null)
                return JsonConvert.SerializeObject(new List<ReadingStory>() { cookieItem });

            var list = JsonConvert.DeserializeObject<List<ReadingStory>>(cookie);
            if (list.Any(x => x.StorySlug == story.StorySlug))
            {
                var updateItem = list.FirstOrDefault(x => x.StorySlug == story.StorySlug);
                updateItem.ChapterOrder = chapter.Order;
                updateItem.LatestReading = DateTime.Now;
            }
            else
            {
                list.Add(cookieItem);
                // max length is 5 story reading recently
                if (list.Count() > 5)
                {
                    var removeItem = list.OrderBy(x => x.LatestReading).FirstOrDefault();
                    list.Remove(removeItem);
                }

            }
            return JsonConvert.SerializeObject(list);
        }

        [Route("test.html")]
        public IActionResult Test()
        {

            return ViewComponent(nameof(Trash));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}


