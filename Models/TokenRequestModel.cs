using System;

namespace Models
{
    public class TokenRequestModel
    {
        public String GrantType{get;set;} // password or refresh_token
        public String ClientId {get;set;}
        public String Username {get;set;}
        public String RefreshToken{get;set;}
        public String Password {get;set;}
    }
}