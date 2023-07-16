using Microsoft.AspNetCore.Mvc;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Data;
using truyenchu.Models;
using truyenchu.Service;
using Microsoft.EntityFrameworkCore;

namespace truyenchu.Components
{
    public class StoriesLatest : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly StoryService _service;

        public StoriesLatest(AppDbContext context, StoryService service)
        {
            _context = context;
            _service = service;
        }
        public IViewComponentResult Invoke()
        {
            List<Story> stories = _context.Stories
                            .Include(x => x.StoryCategory)
                            .ThenInclude(sc => sc.Category)
                            .AsQueryable()
                            .OrderByDescending(x => x.DateUpdated)
                            .Take(20)
                            .ToList();
            return View(stories);
        }
    }
}