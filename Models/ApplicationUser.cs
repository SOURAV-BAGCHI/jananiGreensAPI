using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class ApplicationUser:IdentityUser
    {
        /// Since this class is inheriting from IdentityUser
        /// so all the properties of IdentityUser is already present.!--
        /// The below properties are addition to the already existing properties.
        public String Notes{get;set;}
        public Int32 Types{get;set;}
        public String DisplayName {get;set;}
        public virtual List<TokenModel> Tokens{get;set;} // virtual is added so that this 
                                        //property doesnot create any column in the table
    }
}