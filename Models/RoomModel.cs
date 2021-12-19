using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class RoomModel
    {
        [Key]
        public Int64 RoomId{get;set;}
        [Required]
        [MaxLength(200)]
        public String Name{get;set;}
        [Required]
        public Double RatePerDay{get;set;}
        [Required]
        public Boolean IsACAvailable{get;set;}
        [Required]
        public Double ACCharges{get;set;}
        [Required]
        public Double Discount{get;set;}
        [Required]    
        public Int32 MaxNoOfPersons{get;set;}
        public String Features{get;set;}
        public String Description{get;set;}
        public String ImageList{get;set;}
        public String ImageNameList{get;set;}
    }
}