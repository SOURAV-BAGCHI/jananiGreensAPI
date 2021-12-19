using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AccountController:ControllerBase
    {
        //core services of entity framework

        //To register user
        private readonly UserManager<ApplicationUser> _userManager;
        //To signin user
        private readonly SignInManager<ApplicationUser> _signManager;
        //To create JWT Token from app settings present in the appsettings.json
        private readonly AppSettings _appSettings;


        public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signManager, IOptions<AppSettings> appsettings)
        {
            _userManager=userManager;
            _signManager=signManager;
            _appSettings=appsettings.Value;
        }

        // Users can call this action by action name ,not by http verb
        // host/api/controllername/action
        [HttpPost("[action]")]

        public async Task<ActionResult> Register([FromBody] RegisterViewModel formdata)
        {
            // will hold all the errors related to registration
            List<String> errorList= new List<String>();

            var user=new ApplicationUser(){
                Email=formdata.Email,
                UserName=formdata.Username,
                SecurityStamp=Guid.NewGuid().ToString(),
                DisplayName=formdata.DisplayName
            };

            var result=await _userManager.CreateAsync(user,formdata.Password);

            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user,"Admin");
               //Send confirmation email
                return Ok(new{username=user.UserName,email=user.Email,StatusCode=1,message="Registration successful"});

            } 
            else
            {
                foreach(var err in result.Errors)
                {
                    ModelState.AddModelError("",err.Description);
                    errorList.Add(err.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));
        }
    }
}