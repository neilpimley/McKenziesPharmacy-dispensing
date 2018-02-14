using System;
using Pharmacy.Models;
using Pharmacy.Models.Pocos;

namespace Pharmacy.Dispensing.Models
{
    public class ViewOrder
    {
        public OrderPoco Order { get; set; }

        public DateTime StatusChangedDate { get; set; }

        public CustomerPoco Customer { get; set; }
        
        public Shop Shop { get; set; }

    }
}