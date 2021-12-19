using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ClientViewModel;
using Models.ViewModels;

namespace Data
{
    public class ApplicationDbContext:IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Seed();
            builder.Entity<BookingDetailsViewModel>().HasNoKey().ToView(null);
            builder.Entity<BookingDetailsAndStatusViewModel>().HasNoKey().ToView(null);
            builder.Entity<CancelBookingRequestViewModel>().HasNoKey().ToView(null);
        }

        // public DbSet<BookingRequestQueueModel> BookingRequestQueue{get;set;}
        // public DbSet<BookingUserDetailsModel> BookingUserDetails{get;set;}
        // public DbSet<CancelledRoomBookingDetailsModel> CancelledRoomBookingDetails{get;set;}
        // public DbSet<CompletedRoomBookingDetailsModel> CompletedRoomBookingDetails{get;set;}
        // public DbSet<CurrentRoomBookingDetailsModel> CurrentRoomBookingDetails{get;set;}
        // public DbSet<RoomModel> Rooms{get;set;}
        // public DbSet<TokenModel> Tokens{get;set;}

        public DbSet<RoomModel> Rooms{get;set;}
        public DbSet<CurrentRoomBookingDetailsModel> CurrentRoomBookingDetails{get;set;}
        public DbSet<BookingRequestQueueModel> BookingRequestQueue{get;set;}
        public DbSet<BookingDetailsAndStatusModel> BookingDetailsAndStatus{get;set;}
        public DbSet<TokenModel> Tokens{get;set;}
        public DbSet<CancellationRequestModel> CancellationRequests{get;set;}
        public DbSet<UserFeedbackModel> UserFeedbacks{get;set;}
    }
}