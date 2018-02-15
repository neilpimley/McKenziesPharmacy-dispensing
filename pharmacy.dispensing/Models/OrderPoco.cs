using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pharmacy.Models;

namespace Pharmacy.Dispensing.Models
{
    public class OrderPoco : Order
    {
        public IEnumerable<OrderLinePoco> OrderLines { get; set; }
    }
}
