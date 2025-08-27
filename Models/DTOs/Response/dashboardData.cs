using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Response
{
    public class dashboardData
    {
        public int totalProducts { get; set; }
        public int totalCategories { get; set; }
        public int lowStockProducts { get; set; }
    }
}
