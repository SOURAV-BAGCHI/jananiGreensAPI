using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder builder)
        {
             builder.Entity<IdentityRole>().HasData(
                new {Id="1",Name="Admin",NormalizedName="ADMIN"},
                new {Id="2",Name="Customer",NormalizedName="CUSTOMER"},
                new {Id="3",Name="Moderator",NormalizedName="MODERATOR"}
            );
        }
    }
}