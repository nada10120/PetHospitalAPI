
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DataManager;
using Scalar.AspNetCore;
using Repositories;
using Repositories.IRepository;
using Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Utility;
using Utility.DBInitilizer;
using ECommerce513.Utility;
using Stripe;
using Microsoft.AspNetCore.Mvc;

namespace PetHospitalApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("PetHospitalApi")));

            //add repos
            builder.Services.AddScoped<IPetRepository, PetRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IVetRepository,VetRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDBInitilizer, DBInitilizer>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            //builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            //StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();
           
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();

            }
        
            app.UseCors("AllowAll");

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
           

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitilizer>();
                dbInitializer.Initilize();
            }
            app.MapStaticAssets();


            app.MapControllers().WithMetadata(new AreaAttribute("Admin" ),new AreaAttribute("Identity"));
            app.Run();
        }
    }
}
