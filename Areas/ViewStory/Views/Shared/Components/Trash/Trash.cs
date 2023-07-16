using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace truyenchu.Components {
    public class Trash:ViewComponent
    {
        public IViewComponentResult Invoke() {
            return View();
        }
    }
}