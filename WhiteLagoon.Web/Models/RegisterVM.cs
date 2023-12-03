using System.Collections;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WhiteLagoon.Web.Models;

public class RegisterVM
{
    [Required]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }
    public string? ReturnUrl { get; set; }

    public string Role { get; set; }
    [ValidateNever]
    public IEnumerable <SelectListItem> RoleList { get; set; }
}