using Microsoft.AspNetCore.Mvc.Rendering;
using truyenchu.Models;

namespace truyenchu.Area.ViewStory.Model {
    public class ReadingStory
    {
        public string StoryName { get; set;}
        public string StorySlug { get; set;}
        public int ChapterOrder { get; set;}
        public DateTime LatestReading { get; set;}
    }
}