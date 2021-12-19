using System;

namespace Models.ViewModels
{
    public class BookingDetailsViewModel
    {
        public String BookingId{get;set;}
        public String Name{get;set;}
        public String Phone{get;set;}
        public String Email{get;set;}
        public DateTime CreateDate{get;set;}
        public DateTime BookingStartDate{get;set;}
        public DateTime BookingEndDate{get;set;}
        public String StatusList{get;set;}
        public Int16 Status{get;set;}
        public DateTime Checkin{get;set;}
        public DateTime Checkout{get;set;}
        public String RoomOrderDetails{get;set;}
    }
}