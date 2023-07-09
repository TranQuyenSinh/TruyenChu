using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Components.DetailSidebar
{
    public class DetailSidebar : ViewComponent
    {
        public class DetailSidebarData
        {

        }
        public IViewComponentResult Invoke(DetailSidebarData data)
        {
            return View(data);
        }
    }
}