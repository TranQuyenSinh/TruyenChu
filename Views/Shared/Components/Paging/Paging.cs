using System;
using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Components
{
    public class Paging : ViewComponent
    {
        public class PagingOptions
        {
            public int totalItem { get; set; }
            public int CurrentPage { get; set; }
            public int PageSize { get; set; }
            public Func<int?, string> GenerateUrl { get; set; }
        }

        public IViewComponentResult Invoke(PagingOptions options)
        {
            var totalPages = (int)Math.Ceiling((double)options.totalItem / options.PageSize);

            if (options.CurrentPage > totalPages)
                options.CurrentPage = totalPages;
            if (options.CurrentPage < 1)
                options.CurrentPage = 1;

            return View(new
            {
                CurrentPage = options.CurrentPage,
                TotalPages = totalPages,
                GenerateUrl = options.GenerateUrl
            });
        }
    }
}