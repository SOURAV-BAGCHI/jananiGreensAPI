using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public String Email{get;set;}

        [Required]
        [Display(Name="User name")]
        public String Username { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        public String DisplayName{get;set;}
    }
}