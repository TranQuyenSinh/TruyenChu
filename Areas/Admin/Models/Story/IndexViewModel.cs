using truyenchu.Models;

namespace truyenchu.Areas.Admin.Model {
    public class IndexViewModel:Story
    {
        public string FileName { get; set; }
        public string AuthorName { get; set; }
        public List<string> CategoryNames { get; set; }
    }
}