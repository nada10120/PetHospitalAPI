using Microsoft.AspNetCore.Mvc;
using Repositories.IRepository;
using System.Threading.Tasks;
using System;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IVetRepository _vetRepository; // Assuming this exists
        private readonly IUserRepository _userRepository; // Assuming this exists
        private readonly IAppointmentRepository _appointmentRepository; // Assuming this exists

        public DashboardController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IVetRepository vetRepository,
            IUserRepository userRepository,
            IAppointmentRepository appointmentRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _vetRepository = vetRepository ?? throw new ArgumentNullException(nameof(vetRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var products = await _productRepository.GetAsync();
                var categories = await _categoryRepository.GetAsync();
                var vets = await _vetRepository.GetAsync();
                var users = await _userRepository.GetAsync();
                var appointments = await _appointmentRepository.GetAsync();
                var lowStockProducts = products.Count(p => p.StockQuantity < 10);

                var dashboardData = new
                {
                    totalProducts = products.Count(),
                    totalCategories = categories.Count(),
                    lowStockProducts,
                    totalVets = vets.Count(),
                    totalUsers = users.Count(),
                    totalAppointments = appointments.Count()
                };

                Console.WriteLine($"Dashboard data retrieved: TotalProducts={dashboardData.totalProducts}, TotalCategories={dashboardData.totalCategories}, LowStockProducts={dashboardData.lowStockProducts}, TotalVets={dashboardData.totalVets}, TotalUsers={dashboardData.totalUsers}, TotalAppointments={dashboardData.totalAppointments}");
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dashboard data: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Errors = new[] { "Internal server error.", ex.Message } });
            }
        }
    }
}