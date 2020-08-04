using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MT.Infra.Tool.Models
{
    public class Register
    {

        [Required(ErrorMessage = "Please Enter MID e.g. MXXXXXXX")]
        [StringLength(30, MinimumLength = 3)]
        public string MID { get; set; }

        [Required(ErrorMessage = "Please Enter your Name e.g. John Doe")]
        [StringLength(30, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please Enter your Mindtree Location e.g. Mindtree Chennai")]
        [StringLength(35)]
        public string City { get; set; }

        

       
    }
}