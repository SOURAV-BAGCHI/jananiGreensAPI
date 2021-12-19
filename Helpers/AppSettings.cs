using System;

namespace Helpers
{
    public class AppSettings
    {
         //Properties for JWT Token Signature
        public String Site{get;set;}
        public String Audience{get;set;}
        public String ExpireTime{get;set;} //in minutes
        public String Secret{get;set;}  

        // Token Refresh Properties Added
        public String RefreshToken{get;set;}    
        public String ClientId{get;set;}
        public String GrantType{get;set;} 
        public String RefreshTokenExpireTime{get;set;}
        public String MailId{get;set;}
        public String MailPwd{get;set;}
        public String Host{get;set;}
        public Int32 Port{get;set;}
        public String UserName{get;set;}
        public String Password{get;set;}
        public Boolean EnableSsl{get;set;}
        public Double VerificationTimeLimitInHrs{get;set;}
        public String ClientSite{get;set;}
        public Int16 FeedbackTopRecord{get;set;}
        public Int16 PaginationRecordCount{get;set;}
    }
}