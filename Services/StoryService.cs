using truyenchu.Data;
using truyenchu.Models;
using Microsoft.EntityFrameworkCore;
using truyenchu.Utilities;

namespace truyenchu.Service
{
    public class StoryService
    {
        private readonly AppDbContext _context;
        public StoryService(AppDbContext context)
        {
            _context = context;
        }

        public List<Story> GetStoriesInCategory(Category category, bool isFull = false)
        {
            var qr = _context.Stories
                                .Include(x => x.Author)
                                .Include(x => x.Photo)
                                .Include(x => x.StoryCategory)
                                .ThenInclude(x => x.Category)
                                .AsQueryable()
                                .Where(x => x.StoryCategory.Select(c => c.Category).Contains(category));
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
                            .AsQueryable()
                            .Where(story => story.StorySlug.Contains(searchSlug) || story.Author.AuthorSlug.Contains(searchString))
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
                            .AsQueryable()
                            .Where(story => story.Author.AuthorSlug.Contains(searchAuthorSlug))
                            .ToList();
            return stories;
        }
        public int? GetLatestChapter(Story story)
        {
            var latestChap = (from chapter in _context.Chapters
                              where chapter.StoryId == story.StoryId
                              orderby chapter.Order descending
                              select chapter.Order
                    ).FirstOrDefault();
            return latestChap;
        }

    }
}