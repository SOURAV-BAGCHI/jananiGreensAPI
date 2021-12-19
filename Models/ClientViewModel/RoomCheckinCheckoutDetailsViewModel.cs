using System;

namespace Models.ClientViewModel
{
    public class RoomCheckinCheckoutDetailsViewModel
    {
        public Int64 RoomId{get;set;}
        public DateTime Checkin{get;set;}
        public DateTime Checkout{get;set;}
    }
}