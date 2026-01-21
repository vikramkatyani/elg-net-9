using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ELG.Web.Models
{
    #region [Login]
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter valid email address.")]
        [MaxLength(250)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter password.")]
        [MaxLength(250)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ValidateOrganisationViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter valid company number.")]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [Display(Name = "Company")]
        public string Company { get; set; }
    }

    public class ForgetPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter valid email.")]
        [MaxLength(250)]
        [DataType(DataType.Text)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ActivateUserViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Please enter password.")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        //[RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Please enter atleast one lowercase, one uppercase, one numeric, one special character and password must be eight characters or longer.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Please enter atleast one lowercase, one uppercase, one numeric, one special character and password must be eight characters or longer.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm password.")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password should be same.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please enter old password.")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*?])(?=.{8,})", ErrorMessage = "Please enter atleast one lowercase, one uppercase, one numeric, one special character and password must be eight characters or longer.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter new password.")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        //[RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Please enter atleast one lowercase, one uppercase, one numeric, one special character and password must be eight characters or longer.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Please enter atleast one lowercase, one uppercase, one numeric, one special character and password must be eight characters or longer.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please enter confirm password.")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm Password should be same.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
    #endregion
}