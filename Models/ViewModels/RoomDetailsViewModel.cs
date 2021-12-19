using System;

namespace Models.ViewModels
{
    public class RoomDetailsViewModel
    {
        public Int64 RoomId{get;set;}
        public String Name{get;set;}
        public Double RatePerDay{get;set;}
        public Boolean IsSelected{get;set;}
        public Boolean IsACAvailable{get;set;}
        public Boolean IsAvailingAc{get;set;}
        public Double ACCharges{get;set;}
        public Double Discount{get;set;}
        public Int16 NumberOfPersons{get;set;}
        public Int16 MaxNoOfPersons{get;set;}
        public String [] Features{get;set;}
    //    public String Description{get;set;}
        public String [] ImageList{get;set;}
        public String [] ImageNameList{get;set;}
    }
}