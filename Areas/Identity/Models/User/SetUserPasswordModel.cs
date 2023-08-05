using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using truyenchu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace truyenchu.Areas.Identity.Models.UserViewModels
{
  public class SetUserPasswordModel
  {
    [Display(Name = "Mật khẩu mới")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "{0} phải dài {2} đến {1} ký tự.", MinimumLength = 6)]
    [Required(ErrorMessage = "Phải nhập {0}")]
    [RegularExpression(@"^[a-zA-Z0-9!@#\$%\^\&\*\)\(\+=\._-]+$", ErrorMessage = "{0} không phù hợp")]
    public string NewPassword { get; set; }

    [Display(Name = "Xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Lặp lại mật khẩu không chính xác.")]
    [RegularExpression(@"^[a-zA-Z0-9!@#\$%\^\&\*\)\(\+=\._-]+$", ErrorMessage = "{0} không phù hợp")]
    public string ConfirmPassword { get; set; }


  }
}