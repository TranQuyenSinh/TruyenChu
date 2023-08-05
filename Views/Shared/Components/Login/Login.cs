using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using truyenchu.Models;

namespace truyenchu.Components
{
    public class Login : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;
        public Login(UserManager<AppUser> userManager)
        {
            this._userManager = userManager;
        }
        public IViewComponentResult Invoke()
        {
            return View((object)this.User.Identity.Name);
        }
    }
}