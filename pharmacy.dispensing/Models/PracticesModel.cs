using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Models;

namespace Pharmacy.Dispensing.Models
{
    public class PracticesModel 
    {
        public Practice Practice { get; set; }

        public List<Practice> Practices { get; set; }

        public List<Doctor> Doctors { get; set; }
        public List<SelectListItem> DoctorsDropdown { get; set; }


    }
}
