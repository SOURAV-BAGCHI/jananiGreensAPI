using System;

namespace Models
{
    public class TokenResponseModel
    {
        public String token {get;set;}  //jwt token
        public DateTime expiration {get;set;} //expiry datetime
        public String refresh_token {get;set;}  //refresh token
        public String roles {get;set;}  // user role
        public String username {get;set;}   //user name
        public String displayname{get;set;} // display name
    }
}