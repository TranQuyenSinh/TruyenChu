using Microsoft.AspNetCore.Mvc;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace truyenchu.Components
{
    public class StoriesLatest : ViewComponent
    {
        private readonly AppDbContext _context;

        public StoriesLatest(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var stories = _context.Stories
               .Include(x => x.StoryCategory)
               .ThenInclude(sc => sc.Category)
               .Select(s => new Story
               {
                   StoryName = s.StoryName,
                   StorySlug = s.StorySlug,
                   StoryState = s.StoryState,
                   ViewCount = s.ViewCount,
                   LatestChapterOrder = s.LatestChapterOrder,
                   DateUpdated = s.DateUpdated,
                   StoryCategory = s.StoryCategory,
               })
               .OrderByDescending(x => x.DateUpdated)
               .Take(20)
               .ToList();
            return View(stories);
        }
    }
}