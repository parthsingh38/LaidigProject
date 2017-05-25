using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LaidigSystemsC.Models
{
    public class UserAccount
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage ="First Name is Required !")]
        [StringLength(16, ErrorMessage = "Must be between 4 and 16 characters", MinimumLength = 4)]

        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is Required !")]
        [StringLength(16, ErrorMessage = "Must be between 4 and 16 characters", MinimumLength = 4)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is Required !")]       
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@(laidig.com|kinetic.co|gmail.com)$", ErrorMessage = "Invalid domain in email address. The domain must be @laidig.com, @kinetic.co or gmail.com")]
        [Remote("CheckExistingEmail", "Account", ErrorMessage ="Email Already exits !")]
        public string Email { get; set; }


        [Required(ErrorMessage = "UserName is Required !")]
        [StringLength(16, ErrorMessage = "Must be between 6 and 16 characters", MinimumLength = 6)]
        [Remote("CheckExistingUserName", "Account", ErrorMessage = "UserName Already exits !")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is Required !")]
        [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required(ErrorMessage = "Password is Required !")]
        [StringLength(255, ErrorMessage = "Must be between 8 and 255 characters", MinimumLength = 8)]
        [System.Web.Mvc.Compare("password",ErrorMessage ="Please Confirm Your Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public UserStatus userStatus { get; set; }

        public UserRole userTypes { get; set; }

    }

    public enum UserRole
    {
        Admin,
        Uploader,
        Engineering
    }
    public enum UserStatus
    {
        Active,
        InActive
        
    }


}

