using Microsoft.AspNetCore.Mvc.Rendering;
using truyenchu.Models;

namespace truyenchu.Area.ViewStory.Model {
    public class ChapterViewModel
    {
        public Story Story { get; set; }
        public Chapter Chapter { get; set; }
    }
}