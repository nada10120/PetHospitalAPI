﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Request
{
    public class VetRequest
    {
    
        public string Specialization { get; set; }
        public string AvailabilitySchedule { get; set; }

    }

}
