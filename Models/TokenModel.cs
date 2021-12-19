using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class TokenModel
    {
        [Key]
        public Int32 Id{get;set;}

        // The client id where it comes from (which is required to sign the token)
        [Required]
        public String ClientId{get;set;}

        // Value of the token
        [Required]
        public String Value{get;set;}

        // Get the Token Creation Date
        [Required]
        public DateTime CreatedDate{get;set;}
        
        // The user id it was issued to 
        [Required]
        public String UserId{get;set;}

        [Required]
        public DateTime LastModifiedDate {get;set;}

        [Required]
        public DateTime ExpiryTime {get;set;}

        [ForeignKey("UserId")]  // Foreign key links Application user to Token Model
        public virtual ApplicationUser User{get;set;} // virtual is added so that this 
                                        //property doesnot create any column in the table

        // Foreign key "UserId" is present in IdentityUser so is also inherited to ApplicationUser                                
    }
}