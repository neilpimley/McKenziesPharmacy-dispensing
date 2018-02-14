using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Models;

namespace Pharmacy.Dispensing.Models
{

    public class AddScriptModel
    {
        public CollectScript Script { get; set; }

        public Guid TitleID { get; set; }
        public List<SelectListItem> Titles { get; set; }
        public List<SelectListItem> Doctors { get; set; }
        public List<SelectListItem> Shops { get; set; }

        public string Driver { get; set; }
        public bool SendAlert { get; set; }
        public List<SelectListItem> Drivers { get; set; }
    }


}
