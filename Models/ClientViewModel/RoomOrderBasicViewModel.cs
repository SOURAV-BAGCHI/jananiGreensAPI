using System;

namespace Models.ClientViewModel
{
    public class RoomOrderBasicViewModel
    {
        public Int64 RoomId{get;set;}
        public String Name{get;set;}
        public Boolean IsAcAvailed{get;set;}
        public Int32 NumberOfPerson{get;set;}
        public Double RatePerDay{get;set;}
        public Double ACCharges{get;set;}
        public String Image{get;set;}
    }
}