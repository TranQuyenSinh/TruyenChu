using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.Tokens;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Utilities;

namespace truyenchu.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Route("database-manager/[action]")]
    [Area("Database")]
    public class DatabaseManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseManagerController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DatabaseManagerController(AppDbContext dbContext, ILogger<DatabaseManagerController> logger, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = dbContext;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success = await _context.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xóa database thành công" : "Xóa thất bại";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _context.Database.MigrateAsync();
            StatusMessage = "Cập nhật database thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SeedData()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SeedAdminAccountAsync(string password)
        {
            if (string.Compare(password, Const.PASSWORD_TO_SEED_ADMIN) == 0)
            {
                await SeedAdmin();
                return Ok("Seed admin account successfully");
            }
            return Content("Not correct password, please try again!");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SeedDataAsync(string password)
        {
            if (string.Compare(password, Const.PASSWORD_TO_SEED_ADMIN) == 0)
            {
                await SeedAuthor(40);
                await SeedCategory();
                await SeedStories(100);
                await SeedChapter(30);
                return Ok("Seed demo data successfully");
            }
            return Content("Not correct password, please try again!");
        }

        private async Task SeedAdmin()
        {
            // seed roles
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename = r.GetRawConstantValue().ToString();
                var found = await _roleManager.FindByNameAsync(rolename);
                if (found is null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }
            }

            // add user admin
            var useradmin = await _userManager.FindByNameAsync("admin");
            if (useradmin == null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin"
                };

                await _userManager.CreateAsync(useradmin, "123123");
                await _userManager.AddToRolesAsync(useradmin, new[] { RoleName.Administrator });
            }
        }

        private async Task SeedAuthor(int count)
        {
            _context.Authors.RemoveRange(_context.Authors);
            await _context.SaveChangesAsync();

            // Phát sinh categories mẫu
            var fakerAuthor = new Faker<Author>();
            int cm = 1;
            fakerAuthor.RuleFor(c => c.AuthorName, fk => fk.Commerce.ProductName().ToString());

            List<Author> authors = new List<Author>();
            for (int i = 0; i < count; i++)
            {
                var author = fakerAuthor.Generate();
                author.AuthorName += i;
                author.AuthorSlug = AppUtilities.GenerateSlug(author.AuthorName);
                authors.Add(author);
            }

            _context.Authors.AddRange(authors);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCategory()
        {
            // xóa các categories, post cũ trước khi seed
            _context.Categories.RemoveRange(_context.Categories);
            await _context.SaveChangesAsync();

            // Phát sinh categories mẫu
            var categoryNames = new List<string>() { "Tiên Hiệp", "Kiếm Hiệp", "Ngôn Tình", "Đam Mỹ", "Quan Trường", "Võng Du", "Khoa Huyễn", "Hệ Thống", "Huyền Huyễn", "Dị Giới", "Dị Năng", "Quân Sự", "Lịch Sử", "Xuyên Không", "Xuyên Nhanh", "Trọng Sinh", "Trinh Thám", "Thám Hiểm", "Linh Dị", "Ngược", "Sủng", "Cung Đấu", "Nữ Cường", "Gia Đấu", "Đông Phương", "Đô Thị", "Bách Hợp", "Hài Hước", "Điền Văn", "Cổ Đại", "Mạt Thế", "Truyện Teen", "Phương Tây", "Nữ Phụ", "Light Novel", "Việt Nam", "Đoản Văn", "Khác" };
            var fakerCategory = new Faker<Category>();
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentences(5));

            List<Category> categories = new List<Category>();
            foreach (var name in categoryNames)
            {
                var category = fakerCategory.Generate();
                category.CategoryName = name;
                string slug = AppUtilities.GenerateSlug(name);
                category.CategorySlug = slug;
                categories.Add(category);
            }
            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
        }

        private StoryPhoto GenerateStoryPhoto(string photoDemoName)
        {
            string filePath = Path.Combine(Const.STORY_THUMB_PATH, photoDemoName);
            _logger.LogInformation(filePath);
            byte[] bytes;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
            }

            MemoryStream memoryStream = new MemoryStream(bytes);
            IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "name", "filename.jpg");
            return new StoryPhoto()
            {
                FileName = AppUtilities.UploadPhoto(formFile)
            };
        }
        private void DeleteAllPhotoUpload(List<string> exceptFile)
        {
            FileInfo[] files = new DirectoryInfo(Const.STORY_THUMB_PATH).GetFiles(); // Lấy danh sách các file trong thư mục

            foreach (FileInfo file in files) 
            {
                if (!exceptFile.Contains(file.Name)) 
                {
                    file.Delete(); 
                }
            }
        }

        private async Task SeedStories(int count)
        {
            var demoImages = new List<string>() { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "6.jpg", "8.jpg" };

            // xóa các categories, post cũ trước khi seed
            _context.StoryCategories.RemoveRange(_context.StoryCategories);
            _context.Stories.RemoveRange(_context.Stories);
            await _context.SaveChangesAsync();
            DeleteAllPhotoUpload(demoImages);

            var storyFaker = new Faker<Story>();
            var photos = await _context.StoryPhotos.ToListAsync();
            var authors = await _context.Authors.ToListAsync();
            storyFaker.RuleFor(c => c.StoryName, fk => fk.Commerce.ProductName());
            storyFaker.RuleFor(c => c.Author, fk => fk.PickRandom(authors));
            storyFaker.RuleFor(c => c.Description, fk => fk.Lorem.Paragraphs(7));
            storyFaker.RuleFor(c => c.StorySource, fk => fk.Internet.UrlWithPath());
            storyFaker.RuleFor(c => c.StoryState, fk => fk.Random.Bool());
            storyFaker.RuleFor(c => c.DateCreated, fk => fk.Date.Between(new DateTime(2023, 1, 1), DateTime.Now.Subtract(new TimeSpan(90, 0, 0))));
            storyFaker.RuleFor(c => c.Photo, fk => fk.PickRandom(photos));

            var rand = new Random();
            List<Story> stories = new List<Story>();
            List<StoryCategory> storyCategories = new List<StoryCategory>();

            var categoriesArr = await _context.Categories.ToArrayAsync();

            for (var i = 0; i < count; i++)
            {
                var story = storyFaker.Generate();
                story.StoryName += i;
                story.StorySlug = AppUtilities.GenerateSlug(story.StoryName);
                story.DateUpdated = story.DateCreated.AddDays(rand.Next(2)).AddHours(rand.Next(24)).AddMinutes(rand.Next(60)).AddSeconds(rand.Next(60));
                story.ViewCount = rand.Next(0, 20000);
                story.LatestChapterOrder = 0;
                story.Published = true;

                var photo = GenerateStoryPhoto(demoImages[rand.Next(0, demoImages.Count - 1)]);
                _context.StoryPhotos.Add(photo);
                story.Photo = photo;

                stories.Add(story);

                var addedCates = new List<int>();
                var catesCount = rand.Next(1, 5);
                var y = 0;
                var randomIndex = 0;
                while (y < catesCount)
                {
                    randomIndex = rand.Next(categoriesArr.Count() - 1);
                    if (addedCates.Contains(randomIndex))
                        continue;
                    storyCategories.Add(new StoryCategory() { Story = story, Category = categoriesArr[randomIndex] });
                    addedCates.Add(randomIndex);
                    y++;
                }
            }


            _context.Stories.AddRange(stories);
            _context.StoryCategories.AddRange(storyCategories);

            await _context.SaveChangesAsync();
        }

        private async Task SeedPhotos()
        {
            var fileNames = new List<string>() { "1.jpg", "2.jpg", "3.webp", "4.jpg", "5.webp", "6.jpg", "7.webp" };
            fileNames.ForEach(item =>
             _context.StoryPhotos.Add(new StoryPhoto() { FileName = item })
           );
            await _context.SaveChangesAsync();
        }

        private async Task SeedChapter(int maxChapterCount)
        {
            // xóa các chapter cũ trước khi seed
            _context.Chapters.RemoveRange(_context.Chapters);

            var fakerChapter = new Faker<Chapter>();
            fakerChapter.RuleFor(c => c.Title, fk => fk.Lorem.Sentence(4));

            // mỗi chapter có 2 đoạn văn (demo)
            fakerChapter.RuleFor(c => c.Content, fk =>
            {
                var content = "";
                for (var i = 0; i < 2; i++)
                {
                    content += "<p>" + fk.Lorem.Paragraph() + "</p>";
                }
                return content;
            });

            var rand = new Random();
            List<Story> stories = await _context.Stories.ToListAsync();
            foreach (var story in stories)
            {
                var sochuong = rand.Next(maxChapterCount);
                for (var i = 0; i < sochuong; i++)
                {
                    var chapter = fakerChapter.Generate();
                    chapter.Order = i + 1;
                    chapter.Story = story;
                    chapter.DateCreated = story.DateCreated.AddHours(rand.Next(100)).AddMinutes(rand.Next(60)).AddSeconds(rand.Next(60));
                    _context.Chapters.Add(chapter);
                }
                story.LatestChapterOrder = sochuong;
            }
            await _context.SaveChangesAsync();
        }
    }
}