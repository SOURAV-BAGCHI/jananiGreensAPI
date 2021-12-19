using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Data;
using Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class TokenController:ControllerBase
    {
        // JWT and refresh tokens

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly TokenModel _token;
        private readonly ApplicationDbContext _db;

        public TokenController(UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings,TokenModel token, ApplicationDbContext db)
            {
                _userManager=userManager;
                _appSettings=appSettings.Value;
                _token=token;
                _db=db;
            }

        [HttpPost("[action]")]
        public async Task<ActionResult> Auth([FromBody] TokenRequestModel model) //Granttype can be "password" or "refresh_token"
        {
            // this method will be called in case of login using password and also during refresh token request
           if(model==null)
            {
                return new StatusCodeResult(500);
            }

            switch(model.GrantType)
            {
                case "password":
                    return await GenerateTokenModel(model);
                case "refresh_token":
                    return await RefreshToken(model);
                default:
                    // not supported. return a Http 401 unauthorized
                    return new UnauthorizedResult();
            }
        }

        // Method to create new JWT and refresh token
        private async Task<ActionResult> GenerateTokenModel(TokenRequestModel model)
        {
            // check if there is an user with the given username
            var user= await _userManager.FindByNameAsync(model.Username);

            if(user!=null && await _userManager.CheckPasswordAsync(user,model.Password))
            {
                // if email validation is enabled
                // check if email confirmation is done
                // if(! await _userManager.IsEmailConfirmedAsync(user))
                // {
                //     ModelState.AddModelError(string.Empty,"User has not confirmed email");
                //     return Unauthorized(new {LoginErrorMessage="We have sent you a confirmation email.Please confirm your registration to Log In"});
                // }

                // username and password matches. Create the refresh token
                var newRToken=CreateRefreshToken(_appSettings.ClientId,user.Id);

                // first we delete any existing old refresh token
                var oldRToken= _db.Tokens.Where(rt => rt.UserId == user.Id);

                if(oldRToken !=null)
                {
                    foreach(var oldrt in oldRToken)
                    {
                        _db.Tokens.Remove(oldrt);
                    }
                }
                // Add new refresh token to database
                _db.Tokens.Add(newRToken);

                await _db.SaveChangesAsync();

                // Create and return the access token which contains the refresh and JWT TOKEN
                var accessToken= await CreateAccessToken(user,newRToken.Value);

                return Ok(new{authToken=accessToken});
            }
            ModelState.AddModelError(String.Empty,"Username/Password not found");
            return Unauthorized(new{LoginError="Please check login credentials.Invalid Username/Password was entered"});
        }

        // Create access token
        private async Task<TokenResponseModel> CreateAccessToken(ApplicationUser user,String refreshToken)
        {
            var roles= await _userManager.GetRolesAsync(user);
            var key= new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));
            Double TokenExpiryTime=Convert.ToDouble(_appSettings.ExpireTime);

            var tokenHandler=new JwtSecurityTokenHandler();
            var tokenDescriptor=new SecurityTokenDescriptor{
                    //contains the claims reqd in the future
                    Subject =new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub,user.UserName), // holds the user's identity
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()) ,// holds unique identifier for our json web token
                        new Claim(ClaimTypes.NameIdentifier,user.Id), // holds user id
                        new Claim(ClaimTypes.Role,roles.FirstOrDefault()) ,  // holds the role of user
                        new Claim("LoggedOn",DateTime.Now.ToString())
                    }),
                    SigningCredentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256Signature),
                    Issuer=_appSettings.Site,
                    Audience=_appSettings.Audience,
                    Expires=DateTime.UtcNow.AddMinutes(TokenExpiryTime)
                };

            // Generate JWT Token
            var newToken=tokenHandler.CreateToken(tokenDescriptor);
            var encodedToken=tokenHandler.WriteToken(newToken);

            return new TokenResponseModel(){
                token=encodedToken,
                expiration=newToken.ValidTo,
                roles=roles.FirstOrDefault(),
                username=user.UserName,
                displayname=user.DisplayName,
                refresh_token=refreshToken
            }  ;  
        }

        private TokenModel CreateRefreshToken(String clientId,String userId)
        {
            return new TokenModel(){
                ClientId=clientId,
                UserId=userId,
                CreatedDate=DateTime.UtcNow,
                ExpiryTime=DateTime.UtcNow.AddMinutes(Convert.ToDouble(_appSettings.RefreshTokenExpireTime)),
                Value=Guid.NewGuid().ToString("N")
            };
        }

        // Method to Refresh JWT and Refresh Token
        private async Task<ActionResult> RefreshToken(TokenRequestModel model)
        {
            try{
               // check if the received refreshToken exists for the given clientId
               var rt=_db.Tokens.FirstOrDefault(t=>
               t.ClientId==_appSettings.ClientId &&
               t.Value==model.RefreshToken.ToString());

               if(rt==null)
               {
                // refresh token not found or invalid (or invalid clientId)
                   return new UnauthorizedResult();
               }

                // check if refresh token is expired
               if(rt.ExpiryTime<DateTime.UtcNow)
               {
                   return new UnauthorizedResult();
               }
                // check if there's an user with the refresh token's userId
               var user= await _userManager.FindByIdAsync(rt.UserId);

               if(user==null)
               {
                   // UserId not found or invalid
                   return new UnauthorizedResult();
               }

                // generate a new refresh token 
                var RTokenNew=CreateRefreshToken(rt.ClientId,user.Id);
                // invalidate the old refresh token (by deleting it)
                _db.Tokens.Remove(rt);
                // add the new refresh token
                _db.Tokens.Add(RTokenNew);
                // persist changes in the DB
                await _db.SaveChangesAsync();

                var ResponseToken=await CreateAccessToken(user,RTokenNew.Value);

                return Ok(new{authToken=ResponseToken});

            }
            // catch(Exception ex)
            catch(Exception)
            {
                return new UnauthorizedResult();
            }

        }
    
    }
}