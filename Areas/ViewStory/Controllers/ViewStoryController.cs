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
            if (Request.Cookies[Const.READING_STORY_COOKIE_NAME] != null)
            {
                var list = JsonConvert.DeserializeObject<List<ReadingStory>>(Request.Cookies[Const.READING_STORY_COOKIE_NAME]);
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
            var story = _context.Stories.FirstOrDefault(x => x.StorySlug == storySlug);
            if (story == null)
                return NotFound();

            var chapter = _context.Chapters.FirstOrDefault(x => x.StoryId == story.StoryId && x.Order == chapterOrder);
            if (chapter == null)
            {
                _logger.LogInformation("chapter null");
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


        [Route("tim-kiem/{searchString?}")]
        public async Task<IActionResult> SearchStory(string searchString)
        {
            List<StoryChapterModel> vm = new List<StoryChapterModel>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var stories = _storyService.FindStoriesBySearchString(searchString);
                foreach (var story in stories)
                {
                    vm.Add(new StoryChapterModel()
                    {
                        Story = story,
                        LatestChapter = _storyService.GetLatestChapter(story)
                    });
                }
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

            List<StoryChapterModel> vm = new List<StoryChapterModel>();
            foreach (var story in stories)
            {
                vm.Add(new StoryChapterModel()
                {
                    Story = story,
                    LatestChapter = _storyService.GetLatestChapter(story)
                });
            }

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = category != null ? category.CategoryName: "Tất cả" ,
                    IsActive = true}
            };
            ViewBag.currentCategory = category;
            ViewData["Title"] = category != null ? "TRUYỆN " + category.CategoryName.ToUpper() : "THỂ LOẠI";
            return View(nameof(SearchStory), vm);
        }

        [HttpGet]
        [Route("tac-gia/{authorSlug?}")]
        public async Task<IActionResult> SearchAuthor(string authorSlug)
        {
            if (string.IsNullOrEmpty(authorSlug))
                return NotFound();
            var author = _context.Authors.FirstOrDefault(x => x.AuthorSlug.Contains(authorSlug));

            List<Story> stories = _storyService.FindStoriesByAuthor(author);

            List<StoryChapterModel> vm = new List<StoryChapterModel>();
            foreach (var story in stories)
            {
                vm.Add(new StoryChapterModel()
                {
                    Story = story,
                    LatestChapter = _storyService.GetLatestChapter(story)
                });
            }

            ViewBag.breadcrumbs = new List<BreadCrumbModel>(){
                new BreadCrumbModel() {},
                new BreadCrumbModel() {
                    DisplayName = author.AuthorName ,
                    IsActive = true}
            };

            ViewData["Title"] = "Tác giả " + author.AuthorName;
            return View(nameof(SearchStory), vm);
        }

        [Route("test.html")]
        public IActionResult Test()
        {
            // var imgs = new List<string>() {
            //     "1.jpg", "2.jpg", "3.webp", "4.jpg", "5.webp", "6.jpg", "7.webp"
            // };
            // imgs.ForEach(img =>
            // {
            //     _dbContext.StoryPhotos.Add(new StoryPhoto()
            //     {
            //         FileName = img
            //     });
            // });
            // _dbContext.SaveChanges();
            // var rand = new Random();
            // var photos = _dbContext.StoryPhotos.ToList();
            // stories.ForEach(story => story.Photo = photos[rand.Next(0, photos.Count - 1)]);
            // _dbContext.SaveChanges();

            // var rand = new Random();
            // var stories = _dbContext.Stories.ToList();
            // stories.ForEach(story => story.ViewCount = rand.Next(0, 20000));
            // _dbContext.SaveChanges();

            return View();
        }

        [Route("api/get-stories-by-category-slug")]
        public async Task<IActionResult> GetStoriesByCategorySlug(string type, int count, string categorySlug = "all")
        {
            List<Story> data = null;
            if (categorySlug == "all")
            {
                var qrAll = _context.Stories.Include(x => x.Photo).OrderByDescending(x => x.ViewCount);
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


