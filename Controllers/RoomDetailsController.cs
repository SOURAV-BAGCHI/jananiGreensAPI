using System;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Newtonsoft.Json;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomDetailsController:ControllerBase
    {
        private ApplicationDbContext _db;

        public RoomDetailsController(ApplicationDbContext db)
        {
            _db=db;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetRoomDetails([FromBody] RoomDetailsViewModel formData)
        {
            var data=new RoomModel(){
            //    RoomId=DateTime.Now.Ticks,
                Name=formData.Name,
                RatePerDay=formData.RatePerDay,
                IsACAvailable=formData.IsACAvailable,
                ACCharges=formData.ACCharges,
                Discount=formData.Discount,
                MaxNoOfPersons=formData.MaxNoOfPersons,
                Features=JsonConvert.SerializeObject(formData.Features),
                Description=String.Empty,
                ImageList=JsonConvert.SerializeObject(formData.ImageList),
                ImageNameList=JsonConvert.SerializeObject(formData.ImageNameList)
            };

            await _db.Rooms.AddAsync(data);
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("Room details added successfully"));
        }
        
    }
}