using truyenchu.Data;
using truyenchu.Models;
using Microsoft.EntityFrameworkCore;
using truyenchu.Utilities;

namespace truyenchu.Service
{
    public class StoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StoryService> _logger;
        public StoryService(AppDbContext context, ILogger<StoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Story> GetStoriesInCategory(Category category, bool isFull = false)
        {
            var qr = _context.Stories
                                .Include(x => x.Author)
                                .Include(x => x.Photo)
                                .Include(x => x.StoryCategory)
                                .ThenInclude(x => x.Category)
                                .Where(x => x.Published && x.StoryCategory.Select(c => c.Category).Contains(category));
            if (isFull)
                qr = qr.Where(x => x.StoryState == true);
            return qr.OrderByDescending(x => x.ViewCount).ToList();
        }

        public List<Story> FindStoriesBySearchString(string searchString)
        {
            var searchSlug = AppUtilities.GenerateSlug(searchString);
            var stories = _context.Stories
                            .Include(x => x.Author)
                            .Include(x => x.Photo)
                            .Include(x => x.StoryCategory)
                            .ThenInclude(x => x.Category)
                            .Where(story => (story.Published == true) && (story.StorySlug.Contains(searchSlug) || story.Author.AuthorSlug.Contains(searchString)))
                            .ToList();
            return stories;
        }
        public List<Story> FindStoriesByAuthor(Author author)
        {
            var searchAuthorSlug = author.AuthorSlug;
            var stories = _context.Stories
                            .Include(x => x.Author)
                            .Include(x => x.Photo)
                            .Include(x => x.StoryCategory)
                            .ThenInclude(x => x.Category)
                            .Where(story => story.Published && story.Author.AuthorSlug == searchAuthorSlug)
                            .ToList();
            return stories;
        }
        public List<Story> FindStoryByRangeChapter(int start, int end = int.MaxValue)
        {
            var stories = _context.Stories.Where(x => x.Published && x.LatestChapterOrder >= start && x.LatestChapterOrder <= end)
                                            .Include(x => x.Author)
                                            .Include(x => x.Photo)
                                            .Include(x => x.StoryCategory)
                                            .ThenInclude(sc => sc.Category)
                                            .ToList();
            return stories;
        }

    }
}