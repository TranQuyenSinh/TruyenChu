// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using truyenchu.Areas.Identity.Models.UserViewModels;
using truyenchu.Data;
using truyenchu.ExtendMethods;
using truyenchu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using truyenchu.Utilities;

namespace truyenchu.Areas.Identity.Controllers
{

    [Authorize(Roles = RoleName.Administrator)]
    [Area("Identity")]
    [Route("/ManageUser/[action]")]
    public class UserController : Controller
    {

        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /ManageUser/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> GetUsersApi(string searchStr = "", int pageNumber = 1)
        {
            var qr = _userManager.Users.OrderBy(u => u.UserName);
            var users = await qr.Where(x => x.UserName.Contains(searchStr)).ToListAsync();
            var user_roles = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user_roles.Add(new
                {
                    id = user.Id,
                    userName = user.UserName,
                    roles = string.Join(", ", roles)
                });
            }

            var result = Pagination.PagedResults(user_roles, pageNumber, Const.USERS_PER_PAGE_ADMIN);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            var roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
            ViewBag.AllRoles = roles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateUserModel model)
        {
            var newUser = new AppUser() { UserName = model.UserName };

            var result = await _userManager.CreateAsync(newUser, model.PassWord);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(result);
                _logger.LogError(string.Join(", ", result.Errors.Select(x => x.Description)));
                var roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
                ViewBag.AllRoles = roles;
                return View(model);
            }

            await _userManager.AddToRolesAsync(newUser, model.Roles);

            StatusMessage = "Thêm tài khoản mới thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userManager.DeleteAsync(user);
            StatusMessage = $"Xóa tài khoản \"{user.UserName}\" thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ManageUser/AddRole/id
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {
            var model = new AddUserRoleModel();
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            // role of user
            model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray<string>();

            // all role
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            var OldRoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();

            // roles need delete
            var deleteRoles = OldRoleNames.Where(r => !model.RoleNames.Contains(r));

            // role need add
            var addRoles = model.RoleNames.Where(r => !OldRoleNames.Contains(r));

            // all roles
            ViewBag.allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user, deleteRoles);
            if (!resultDelete.Succeeded)
            {
                ModelState.AddModelError(resultDelete);
                return View(model);
            }

            var resultAdd = await _userManager.AddToRolesAsync(model.user, addRoles);
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError(resultAdd);
                return View(model);
            }

            StatusMessage = $"Vừa cập nhật quyền cho user: {model.user.UserName}";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ChangePasswordAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            ViewBag.user = user;
            return View();
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordAsync(string id, SetUserPasswordModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = user;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            StatusMessage = $"Vừa cập nhật mật khẩu cho user: {user.UserName}";
            return RedirectToAction(nameof(Index));
        }



    }
}
