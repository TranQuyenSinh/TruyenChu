using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Components.DetailSidebar
{
    public class DetailSidebar : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}