using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class BookingDetailsAndStatusModel
    {
        [Key]
        public String BookingId{get;set;}
        
        public DateTime Checkin{get;set;}
        public DateTime Checkout{get;set;}
        [Required]
        public Int16 Status{get;set;}   
        // 1- verified, 2- payment made, 3 checked in, 4 checked out, 0- cancel
        public String StatusList{get;set;}
        public String RoomCheckinCheckoutDetails{get;set;}
        public Boolean IsProcessComplete{get;set;}
        public DateTime BookingStartDate{get;set;}
        public String CustomerName{get;set;}
    }
}