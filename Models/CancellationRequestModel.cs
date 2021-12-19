using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class CancellationRequestModel
    {
        [Key]
        [MaxLength(200)]
        public String BookingRequestId{get;set;}
        [Required]
        [MaxLength(200)]
        public String CustomerName{get;set;}
        [Required]
        public DateTime RequestDateTime{get;set;}
        [Required]
        public DateTime BookingStartDate{get;set;}
        [MaxLength(1000)]
        public String Reason{get;set;}
        [Required]
        public Boolean CancellationAccepted{get;set;}
        
    }
}