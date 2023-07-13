using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using truyenchu.Data;
using truyenchu.Models;

namespace truyenchu.Components
{
    public class ChapterNavigation : ViewComponent
    {
        private readonly AppDbContext _context;
        public ChapterNavigation(AppDbContext context)
        {
            _context = context;
        }
        public class ChapterNavigationData
        {
            public Story Story { get; set; }
            public Chapter Chapter { get; set; }
            public SelectList selectListItems { get; set; }
            public int PreviousChapter { get; set; }
            public int NextChapter { get; set; }
        }
        public IViewComponentResult Invoke(Story story, Chapter chapter)
        {
            var previousChapter = chapter.Order - 1;
            var lastChapter = _context.Chapters.Where(x => x.StoryId == story.StoryId).OrderByDescending(x => x.Order).FirstOrDefault();
            var nextChapter = chapter.Order + 1;
            if (nextChapter > lastChapter.Order)
                nextChapter = 0;

            var allChapters = _context.Chapters
                            .Where(c => c.StoryId == story.StoryId)
                            .OrderBy(c => c.Order).ToList();

            var selectListItems = new SelectList(
                allChapters.Select(c => new { Title = "Chương " + c.Order, Order = c.Order }),
                nameof(chapter.Order),
                nameof(chapter.Title),
                chapter.Order
            );

            var data = new ChapterNavigationData()
            {
                Chapter = chapter,
                Story = story,
                selectListItems = selectListItems,
                NextChapter = nextChapter,
                PreviousChapter = previousChapter
            };

            return View(data);
        }
    }
}