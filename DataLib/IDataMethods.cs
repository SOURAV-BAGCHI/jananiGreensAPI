using System;
using Data;
using Models.ViewModels;

namespace DataLib
{
    public interface IDataMethods
    {
        void InitiateBookingDetailsAndStatus(Object Obj,ApplicationDbContext _db);
        BookingDetailsViewModel GetBookingDetails(String BookingId,ApplicationDbContext _db);
    }
}