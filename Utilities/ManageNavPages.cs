namespace truyenchu.Utilities
{
    public static class ManageNavPages
    {
        public static string Author => "Author";
        public static string AuthorNavClass(string activePage) => PageNavClass(activePage, Author);

        public static string Category => "Category";
        public static string CategoryNavClass(string activePage) => PageNavClass(activePage, Category);

        public static string Story => "Story";
        public static string StoryNavClass(string activePage) => PageNavClass(activePage, Story);

        public static string Chapter => "Chapter";
        public static string ChapterNavClass(string activePage) => PageNavClass(activePage, Chapter);

        public static string User => "User";
        public static string UserNavClass(string activePage) => PageNavClass(activePage, User);

        // public static string Author => "Author";
        // public static string AuthorNavClass(string activePage) => PageNavClass(activePage, Author);

        public static string PageNavClass(string activePage, string page)
        {
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
