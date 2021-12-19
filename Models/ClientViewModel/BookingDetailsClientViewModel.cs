using System;
using System.Collections.Generic;
using Models.ViewModels;

namespace Models.ClientViewModel
{
    public class BookingDetailsClientViewModel
    {
        public String BookingId{get;set;}
        public String Name{get;set;}
        public String Phone{get;set;}
        public String Email{get;set;}
        public DateTime CreateDate{get;set;}
        public DateTime BookingStartDate{get;set;}
        public DateTime BookingEndDate{get;set;}
        public List<StatusRecordViewModel> StatusList{get;set;}
        public Int16 Status{get;set;}
        public DateTime Checkin{get;set;}
        public DateTime Checkout{get;set;}
        public List<RoomOrderBasicViewModel> RoomOrderDetails{get;set;}
        public Int32 NumberOfDays{get;set;}
    }
}