using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using Models.ViewModels;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController:ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;

        private enum Status{
            Cancel,
            Verified,
            PaymentDone,
            Checkedin,
            Checkedout
        }
        public FeedbackController(ApplicationDbContext db,IOptions<AppSettings> appSettings)
        {
            _db=db;
            _appSettings=appSettings.Value;
        } 

        [HttpPost("[action]")]
        public async Task<IActionResult> SetFeedback([FromBody] FeedbackViewModel formData)
        {
            var BookingDetailsAndStatus=await _db.BookingDetailsAndStatus.FindAsync(formData.BookingId);

            if(BookingDetailsAndStatus==null)
            {
                return Unauthorized();
            }
            else if(BookingDetailsAndStatus.Status!=(Int16)Status.Checkedout)
            {
                return Unauthorized();
            }
            else
            {
                var FeedbackPresent= _db.UserFeedbacks.Where(x=>x.BookingId==formData.BookingId).Count();
                if(FeedbackPresent==0)
                {
                    var UserFeedback=new UserFeedbackModel()
                    {
                        Rating=formData.Rating,
                        CustomerName=BookingDetailsAndStatus.CustomerName,
                        ReviewTitle=formData.ReviewTitle,
                        Review=formData.Review,
                        BookingId=formData.BookingId,
                        FeedbackDate=DateTime.Now
                    };

                    await _db.UserFeedbacks.AddAsync(UserFeedback);
                    await _db.SaveChangesAsync();

                    return Ok(1);
                }
                else
                {
                    return Ok(0); //BadRequest();
                }
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetFeedbacks()
        {
            var Feedbacks=_db.UserFeedbacks.OrderByDescending(x=>x.FeedbackId).Take(_appSettings.FeedbackTopRecord)
            .Select(x=>new {
                x.Rating,
                x.CustomerName,
                x.ReviewTitle,
                x.Review,
                x.FeedbackDate
            })
            .ToList();

            return Ok(Feedbacks);
        }
    
    }
}