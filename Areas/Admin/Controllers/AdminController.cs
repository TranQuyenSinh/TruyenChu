using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using truyenchu.Areas.Admin.Model.Admin;
using truyenchu.Data;

namespace truyenchu.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Route("/Admin/[action]")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new IndexViewModel()
            {
                TotalStory = _context.Stories.Count(),
                TotalAuthor = _context.Authors.Count(),
                TotalStoryUpdateToday = _context.Stories.Where(x=>x.DateUpdated >= DateTime.Now.Date).Count()
            };
            return View(model);
        }
    }
}