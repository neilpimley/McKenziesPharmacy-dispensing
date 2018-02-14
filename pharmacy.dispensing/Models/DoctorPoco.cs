using Pharmacy.Models;

namespace Pharmacy.Dispensing.Models
{
    public class DoctorPoco : Doctor
    {
        public Title Title { get; set; }
        public Practice Practice { get; set; }
    }
}