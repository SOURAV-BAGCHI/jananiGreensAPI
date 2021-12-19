using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class BookingRequestQueueModel
    {
        [Key]
        public String BookingRequestId{get;set;}
        [Required]
        [MaxLength(200)]
        public String Name{get;set;}
        
        [MaxLength(20)]
        public String Phone{get;set;}
        
        [MaxLength(500)]
        public String Email{get;set;}

        [Required]
        public DateTime BookingStartDate{get;set;}

        [Required]
        public DateTime BookingEndDate{get;set;}
        public String RoomOrderDetails{get;set;}
        public Int64 VerificationCode{get;set;}
        public DateTime VerificationLimit{get;set;}
        public Boolean IsVerified{get;set;}
        public DateTime CreateDate{get;set;}
    }
}