using System;

namespace Models.ViewModels
{
    public class CancelBookingRequestViewModel
    {
        public String BookingRequestId{get;set;}
        public String Name{get;set;}
        public String Email{get;set;}
        public DateTime BookingStartDate{get;set;}
        public DateTime BookingEndDate{get;set;}
    }
}