using System.Collections.Generic;
using System.ComponentModel;
using truyenchu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace truyenchu.Areas.Identity.Models.UserViewModels
{
  public class AddUserRoleModel
  {
    public AppUser user { get; set; }

    [DisplayName("Các role gán cho user")]
    public string[]? RoleNames { get; set; } = new string[] {};

    public List<IdentityRoleClaim<string>> claimsInRole { get; set; }
    public List<IdentityUserClaim<string>> claimsInUserClaim { get; set; }

  }
}