using System;
using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class CancellationRequestViewModel
    {
        [Required]
        public String BookingRequestId{get;set;}
        public String CustomerName{get;set;}
        public String CancellationRequestDate{get;set;}
        public String BookingStartDate{get;set;}
        public String Reason{get;set;}
    }
}