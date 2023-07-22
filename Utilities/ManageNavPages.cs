namespace truyenchu.Utilities
{
    public static class ManageNavPages
    {
        public static string Author => "Author";
        public static string AuthorNavClass(string activePage) => PageNavClass(activePage, Author);
        
        public static string Story => "Story";
        public static string StoryNavClass(string activePage) => PageNavClass(activePage, Story);

        public static string Chapter => "Chapter";
        public static string ChapterNavClass(string activePage) => PageNavClass(activePage, Chapter);

        // public static string Author => "Author";
        // public static string AuthorNavClass(string activePage) => PageNavClass(activePage, Author);

        public static string PageNavClass(string activePage, string page)
        {
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
