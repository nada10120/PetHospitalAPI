
using DataManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Models;


namespace Utility.DBInitilizer
{
    public class DBInitilizer : IDBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public DBInitilizer(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initilize()
        {
            try
            {

                if(_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    _roleManager.CreateAsync(new(SD.SuperAdmin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.Vet)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.Client)).GetAwaiter().GetResult();
                }

                _userManager.CreateAsync(new()
                {
                    Email = "Admin@eraasoft.com",
                    UserName = "Admin",
                    EmailConfirmed = true,
                    Address="Elmahalla",
                    ProfilePicture="temp.jpg",
                    Role=SD.SuperAdmin
                    
                }, "Admin123$").GetAwaiter().GetResult();

                var user = _userManager.FindByEmailAsync("Admin@eraasoft.com").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, SD.SuperAdmin).GetAwaiter().GetResult();
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
            
    }
}
