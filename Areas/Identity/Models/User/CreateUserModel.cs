using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Areas.Identity.Models.UserViewModels
{
    public class CreateUserModel
    {
        [DisplayName("Tên đăng nhập")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "{0} không phù hợp")]
        [StringLength(50, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.", MinimumLength = 3)]
        public string UserName { get; set; }

        [DisplayName("Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9!@#\$%\^\&\*\)\(\+=\._-]+$", ErrorMessage = "{0} không phù hợp")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.", MinimumLength = 6)]
        public string PassWord { get; set; }

        [DisplayName("Các role gán cho user")]
        public string[]? Roles { get; set; } = Array.Empty<string>();
    }
}