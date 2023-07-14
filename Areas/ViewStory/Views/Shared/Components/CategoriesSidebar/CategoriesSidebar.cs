using Microsoft.AspNetCore.Mvc;
using truyenchu.Data;

namespace truyenchu.Components
{
    public class CategoriesSidebar : ViewComponent
    {
        private readonly AppDbContext _context;

        public CategoriesSidebar(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var cates = _context.Categories.ToList();
            return View(cates);
        }
    }
}