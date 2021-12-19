using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class UserFeedbackModel
    {
        [Key]
        public Int64 FeedbackId{get;set;}
        [Required]
        public Int16 Rating{get;set;}
        [Required]
        [MaxLength(200)]
        public String CustomerName{get;set;} 
        [MaxLength(50)]
        public String ReviewTitle{get;set;}
        [MaxLength(500)]
        public String Review{get;set;}
        [Required]
        public String BookingId{get;set;}
        [Required]
        public DateTime FeedbackDate{get;set;}


    }
}