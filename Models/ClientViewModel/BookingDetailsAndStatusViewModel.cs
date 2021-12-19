using System;

namespace Models.ClientViewModel
{
    public class BookingDetailsAndStatusViewModel
    {
        public String BookingId{get;set;}
        public String BookingStartDate{get;set;}
        // public DateTime BookingStartDate{get;set;}
        public String CustomerName{get;set;}
        public Int16 Status{get;set;}
    }
}