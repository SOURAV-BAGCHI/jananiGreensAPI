using System;

namespace Models.ClientViewModel
{
    public class FeedbackClientViewModel
    {
        public Int16 Rating{get;set;}        
        public String CustomerName{get;set;} 
        public String ReviewTitle{get;set;}       
        public String Review{get;set;}        
        public String BookingId{get;set;}        
        public DateTime FeedbackDate{get;set;}
    }
}