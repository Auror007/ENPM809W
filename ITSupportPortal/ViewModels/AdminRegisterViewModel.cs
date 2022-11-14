﻿using ITSupportPortal.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ITSupportPortal.ViewModels
{
    public class AdminRegisterViewModel
    {
        [Display(Name = "Type Of Account")]
        [Required(ErrorMessage = "Please select a type of Account")]
        public EnumRole accountType { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Please enter your user name")]
        public string Username { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Please enter your company email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [MinLength(12)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match")]
        [MinLength(12)]
        public string ConfirmedPassword { get; set; }
    }
}
