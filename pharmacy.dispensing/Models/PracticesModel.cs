using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Models;

namespace Pharmacy.Dispensing.Models
{
    public class PracticesModel 
    {
        public PracticePoco Practice { get; set; }

        public IEnumerable<PracticePoco> Practices { get; set; }

        public IEnumerable<Doctor> Doctors { get; set; }
        public IEnumerable<SelectListItem> DoctorsDropdown { get; set; }


    }
}
