using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class BookingUserDetailsModel
    {
        [Key]
        public Int64 BookingId{get;set;}
        [Required]
        [MaxLength(200)]
        public String Name{get;set;}
        
        [MaxLength(20)]
        public String Phone{get;set;}
        
        [MaxLength(500)]
        public String Email{get;set;}

        [Required]
        public Int16 NumberOfGuests{get;set;}
    }
}