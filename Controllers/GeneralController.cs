using Data;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeneralController:ControllerBase
    {
        private ApplicationDbContext _db;

        public GeneralController(ApplicationDbContext db)
        {
            _db=db;
        }
    }
}