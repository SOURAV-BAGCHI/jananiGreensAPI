using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Newtonsoft.Json;

namespace DataLib
{
    public class DataMethods:IDataMethods
    {
        public void InitiateBookingDetailsAndStatus(Object Obj,ApplicationDbContext _db)
        {
            BookingRequestQueueModel formData=JsonConvert.DeserializeObject<BookingRequestQueueModel>(Obj.ToString());
            List<StatusRecordViewModel> SRVMObjList=new List<StatusRecordViewModel>(){
                new StatusRecordViewModel(){
                    Status=1,
                    StatusDate=DateTime.Now.ToString("dd-MM-yyyy HH:mm")
                }
            };

            var data=new BookingDetailsAndStatusModel()
            {
                BookingId=formData.BookingRequestId,
                Checkin=DateTime.Now,
                Checkout=DateTime.Now,
                Status=1,
                StatusList=JsonConvert.SerializeObject(SRVMObjList),
                RoomCheckinCheckoutDetails=String.Empty,
                IsProcessComplete=false,
                BookingStartDate=formData.BookingStartDate,
                CustomerName=formData.Name
            };

            _db.BookingDetailsAndStatus.Add(data);
            _db.SaveChanges();

            return;
        }

        public BookingDetailsViewModel GetBookingDetails(String BookingId,ApplicationDbContext _db)
        {
            SqlParameter param1 = new SqlParameter("@str_bookingId", BookingId); 
            var bookingDetails =  _db.Set<BookingDetailsViewModel>().FromSqlRaw("usp_GetBookingDetails {0}",param1).ToList().FirstOrDefault();

            return bookingDetails;
        }
    }
}