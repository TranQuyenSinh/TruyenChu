using System;
using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Components
{
    public class ConfirmModal : ViewComponent
    {
        public class Input
        {
            public string Id { get; set; }
            public string Title { get; set; } = "Thông báo";
            public string Content { get; set; }
            public string ReturnUrl { get; set; } = "/";
            public string Action { get; set; }
            public string Area { get; set; }
            public string Controller { get; set; }

        }

        public IViewComponentResult Invoke(Input data)
        {
            return View(data);
        }
    }
}