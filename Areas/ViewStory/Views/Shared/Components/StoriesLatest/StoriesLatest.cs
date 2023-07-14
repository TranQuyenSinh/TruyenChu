using Microsoft.AspNetCore.Mvc;
using truyenchu.Area.ViewStory.Model;
using truyenchu.Data;

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
            var list = new List<StoryChapterModel>();
            var stories = _context.Stories.ToList();
            // stories.ForEach(s=> 
            return View();
        }
    }
}