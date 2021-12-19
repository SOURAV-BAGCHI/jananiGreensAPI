using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name="User name")]
        public String Username { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }
    }
}