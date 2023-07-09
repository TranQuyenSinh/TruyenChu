using System;
using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Components.Paging
{
    public class Paging:ViewComponent
    {
        public class PagingOptions {
            public int currentpage { get; set; }
            public int countpages { get; set; }
            public Func<int?, string> generateUrl { get; set; }
        }
        public IViewComponentResult Invoke(PagingOptions data) {
            return View(data);
        }
    }
}