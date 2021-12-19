using System;
using System.ComponentModel.DataAnnotations;

namespace Models.ClientViewModel
{
    public class BookingRequestViewModel
    {
        public String BookingRequestId{get;set;}
        [Required]
        [MaxLength(200)]
        public String Name{get;set;}
        
        [MaxLength(20)]
        public String Phone{get;set;}
        
        [MaxLength(500)]
        public String Email{get;set;}

        [Required]
        public String BookingStartDate{get;set;}

        [Required]
        public String BookingEndDate{get;set;}
        public String RoomOrderDetails{get;set;}
    }
}