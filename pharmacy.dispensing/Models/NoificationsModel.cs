using System.Collections.Generic;

namespace Pharmacy.Dispensing.Models
{

    public class NoificationsItem
    {
        public string name {get;set;}
        public int items { get; set; }
    }

    public class NoificationsModel
    {
        public List<NoificationsItem> ScriptOrdered { get; set; }
        public List<NoificationsItem> Collected { get; set; }
        public List<NoificationsItem> NotCollected { get; set; }
        public List<NoificationsItem> MissingItems { get; set; }
    }
}