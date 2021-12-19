using System;

namespace Models.ViewModels
{
    public class FeedbackViewModel
    {
        public String BookingId{get;set;}
        public Int16 Rating{get;set;}
        public String ReviewTitle{get;set;}
        public String Review{get;set;}
    }
}