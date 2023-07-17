namespace truyenchu.Utilities
{
    public static class Pagination
    {
        public class PagingData<T> where T : class
        {
            public IEnumerable<T> Data { get; set; }
            public int TotalPages { get; set; }
            public int CurrentPage { get; set; }
        }

        public static PagingData<T> PagedResults<T>(this List<T> list, int pageNumber, int pageSize) where T : class
        {
            var result = new PagingData<T>();
            result.Data = list.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();
            result.TotalPages = Convert.ToInt32(Math.Ceiling((double)list.Count() / pageSize));
            result.CurrentPage = pageNumber;
            return result;
        }
    }
}