using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class CancelledRoomBookingDetailsModel
    {
        [Key]
        public Int64 RoomBookingDetailsId{get;set;}

        [Required]
        public Int64 FkRoomId{get;set;}
        [Required]
        public Int64 FkBookingId{get;set;}

        [Required]
        public DateTime StartDate{get;set;}

        [Required]
        public DateTime EndDate{get;set;}
    }
}