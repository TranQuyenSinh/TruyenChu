using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using truyenchu.Data;
using truyenchu.Models;

namespace truyenchu.Components.DetailSidebar
{
    public class DetailSidebar : ViewComponent
    {
        private readonly AppDbContext _context;
        public DetailSidebar(AppDbContext context)
        {
            _context = context;
        }

        public class DetailSidebarViewModel
        {
            public List<Story> relatedStories { get; set; }
            public List<Story> sameAuthorStories { get; set; }
        }
        public IViewComponentResult Invoke(Story story)
        {
            var vm = new DetailSidebarViewModel();
            var catesOfStory = story.StoryCategory.Select(x => x.Category);
            vm.relatedStories = _context.Stories
                                .Include(s => s.StoryCategory)
                                .ThenInclude(x => x.Category)
                                .Where(s => s.Published && s.StoryId != story.StoryId && s.StoryCategory.Select(c => c.Category).Any(c => catesOfStory.Contains(c)))
                                .Take(10)
                                .OrderByDescending(x => x.ViewCount)
                                .ToList();
            vm.sameAuthorStories = _context.Stories
                                .Include(s => s.StoryCategory)
                                .ThenInclude(x => x.Category)
                                .Where(s => s.Published && s.StoryId != story.StoryId && s.AuthorId == story.AuthorId)
                                .Take(10)
                                .OrderByDescending(x => x.ViewCount)
                                .ToList();

            return View(vm);
        }
    }
}