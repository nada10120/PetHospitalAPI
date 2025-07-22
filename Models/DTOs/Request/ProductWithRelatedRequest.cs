using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTOs.Request;

namespace Models.DTOs.Response
{
    public class ProductWithRelatedRequest
    {
        public FilterItemsRequest FilterItemsRequest { get; set; } = null!;
       
        public double TotalPageNumber { get; set; }

    }
}
