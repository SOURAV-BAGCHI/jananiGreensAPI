using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using DataLib;
using Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;

namespace JANANIGREENS.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors(options=>{
                options.AddPolicy("EnableCORS",builder=>{
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();//AllowCredentials().Build();
                });

            });

            services.AddScoped<TokenModel>();
            services.AddSingleton<IDataMethods,DataMethods>();
            // services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            // Connect to database

            //AddDbContext creates instance of ApplicationDbContext each time a request arrives
            //services.AddDbContext<ApplicationDbContext>(options=> options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")) );
            
            //AddDbContextPool takes any availablle instance from DbContextPool and is better in performance
            services.AddDbContextPool<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Specifying we are going to use Identity Framework
            services.AddIdentity<ApplicationUser,IdentityRole>(options =>{
                options.Password.RequireDigit=true;
                options.Password.RequiredLength=6;
                options.Password.RequireNonAlphanumeric=true;
                options.Password.RequireLowercase=true;
                options.Password.RequireUppercase=true;
                options.User.RequireUniqueEmail=true;

                //Lockout settings
                options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts=5;
                options.Lockout.AllowedForNewUsers=true;

            }

            ).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // Configure strongly typed settings objects
            var appSettingsSection=Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings=appSettingsSection.Get<AppSettings>();
            var key=Encoding.ASCII.GetBytes(appSettings.Secret);

            // Authenticaton Middelware
            services.AddAuthentication(o=>{
                o.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme=JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;  
            }).AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,options=>{
                options.TokenValidationParameters=new TokenValidationParameters{
                    ValidateIssuerSigningKey=true,
                    ValidateIssuer=true,
                    ValidateAudience=true,
                   // RequireExpirationTime=false,
                    ValidIssuer=appSettings.Site,
                    ValidAudience=appSettings.Audience,
                    IssuerSigningKey=new SymmetricSecurityKey(key),
                    ClockSkew=TimeSpan.Zero
                };
            });
        
            // we could also use services.Configure<IdentityOptions>(options =>{
        //    options.Password.RequireDigit=true;
        //  etc......
        //})

            services.AddAuthorization(options=>{
                options.AddPolicy("RequireLoggedIn",policy=> policy.RequireRole("Admin","Customer","Moderator").RequireAuthenticatedUser());//.RequireClaim("abc"));
                options.AddPolicy("RequireAdministratorRole",policy=>policy.RequireRole("Admin").RequireAuthenticatedUser());    
            });

            /*
            Requirment:
            User should be authenticated
            User should be authorized            
            */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();
            app.UseCors("EnableCORS");
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
