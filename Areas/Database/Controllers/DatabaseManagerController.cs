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
    [Route("database-manager/[action]")]
    [Area("Database")]
    public class DatabaseManagerController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DatabaseManagerController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DatabaseManagerController(AppDbContext dbContext, ILogger<DatabaseManagerController> logger, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
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
            var success = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xóa database thành công" : "Xóa thất bại";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "Cập nhật database thành công";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> SeedDataAsync()
        {
            // await SeedAuthor();
            //await SeedCategory();
            // await SeedCategoryAndStory();
            // await SeedChapter();
            await seedAdmin();
            StatusMessage = "Seed database thành công";
            return RedirectToAction(nameof(Index));
        }

        private async Task seedAdmin()
        {
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

            // tạo user admin
            var useradmin = await _userManager.FindByNameAsync("admin");
            if (useradmin == null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(useradmin, "123123");
                await _userManager.AddToRolesAsync(useradmin, new[] { RoleName.Administrator });
            }
        }

        private async Task SeedAuthor()
        {
            // _dbContext.Authors.RemoveRange(_dbContext.Authors);
            // await _dbContext.SaveChangesAsync();

            // Phát sinh categories mẫu
            var fakerAuthor = new Faker<Author>();
            int cm = 1;
            fakerAuthor.RuleFor(c => c.AuthorName, fk => fk.Commerce.ProductName().ToString());

            List<Author> authors = new List<Author>();
            for (int i = 100; i < 200; i++)
            {
                var author = fakerAuthor.Generate();
                author.AuthorName += i;
                author.AuthorSlug = AppUtilities.GenerateSlug(author.AuthorName);
                authors.Add(author);
            }

            _dbContext.Authors.AddRange(authors);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedCategory()
        {
            // xóa các categories, post cũ trước khi seed
            _dbContext.Categories.RemoveRange(_dbContext.Categories);
            await _dbContext.SaveChangesAsync();

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
            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedCategoryAndStory()
        {
            // xóa các categories, post cũ trước khi seed
            _dbContext.StoryCategories.RemoveRange(_dbContext.StoryCategories);
            _dbContext.Stories.RemoveRange(_dbContext.Stories);
            await _dbContext.SaveChangesAsync();

            var storyFaker = new Faker<Story>();
            var photos = await _dbContext.StoryPhotos.ToListAsync();
            var authors = _dbContext.Authors.ToListAsync().Result;
            storyFaker.RuleFor(c => c.StoryName, fk => fk.Commerce.ProductName());
            storyFaker.RuleFor(c => c.Author, fk => fk.PickRandom(authors));
            storyFaker.RuleFor(c => c.Description, fk => fk.Lorem.Paragraphs(7));
            storyFaker.RuleFor(c => c.StorySource, fk => fk.Internet.UrlWithPath());
            storyFaker.RuleFor(c => c.StoryState, fk => fk.Random.Bool());
            storyFaker.RuleFor(c => c.DateCreated, fk => fk.Date.Between(new DateTime(2023, 1, 1), DateTime.Now.Subtract(new TimeSpan(90, 0 , 0))));
            storyFaker.RuleFor(c => c.Photo, fk => fk.PickRandom(photos));
            
            var rand = new Random();
            List<Story> stories = new List<Story>();
            List<StoryCategory> storyCategories = new List<StoryCategory>();
            var categoriesArr = await _dbContext.Categories.ToArrayAsync();
            for (var i = 0; i < 300; i++)
            {
                var story = storyFaker.Generate();
                story.StoryName += i;
                story.StorySlug = AppUtilities.GenerateSlug(story.StoryName);
                story.DateUpdated = story.DateCreated.AddDays(rand.Next(2)).AddHours(rand.Next(24)).AddMinutes(rand.Next(60)).AddSeconds(rand.Next(60));
                story.ViewCount = rand.Next(0, 20000);
                story.LatestChapterOrder = 0;
                stories.Add(story);

                var addedCates = new List<int>();
                var catesCount = rand.Next(1, 5);
                var y = 0;
                var randomIndex = 0;
                while ( y < catesCount)
                {
                    randomIndex = rand.Next(categoriesArr.Count() - 1);
                    if (addedCates.Contains(randomIndex))
                        continue;
                    storyCategories.Add(new StoryCategory() { Story = story, Category = categoriesArr[randomIndex] });
                    addedCates.Add(randomIndex);
                    y++;
                }
            }


            _dbContext.Stories.AddRange(stories);
            _dbContext.StoryCategories.AddRange(storyCategories);

            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedChapter()
        {
            // xóa các categories, post cũ trước khi seed
            _dbContext.Chapters.RemoveRange(_dbContext.Chapters);

            // Phát sinh categories mẫu
            var fakerChapter = new Faker<Chapter>();
            fakerChapter.RuleFor(c => c.Title, fk => fk.Lorem.Sentence(5));
            fakerChapter.RuleFor(c => c.Content, fk =>
            {
                var content = "";
                for (var i = 0; i < 3; i++)
                {
                    content += "<p>" + fk.Lorem.Paragraph() + "</p>";
                }
                return content;
            });

            var rand = new Random();
            List<Story> stories = await _dbContext.Stories.ToListAsync();
            foreach (var story in stories)
            {
                var sochuong = rand.Next(100);
                for (var i = 0; i < sochuong; i++)
                {
                    var chapter = fakerChapter.Generate();
                    chapter.Order = i + 1;
                    chapter.Story = story;
                    chapter.DateCreated = story.DateCreated.AddHours(rand.Next(100)).AddMinutes(rand.Next(60)).AddSeconds(rand.Next(60));
                    _dbContext.Chapters.Add(chapter);
                }
                story.LatestChapterOrder = sochuong;
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}