using System;
using System.Collections.Generic;
using Pharmacy.Models;
using Pharmacy.Models.Pocos;

namespace Pharmacy.Dispensing.Models
{

    public class ScriptCollection 
    {
        public Guid PracticeId { get; set; }
        public string Practice { get; set; }
        public IEnumerable<CollectScriptPoco> Scripts { get; set; }
    }

    public partial class CollectScriptPoco : CollectScript
    {
        public IEnumerable<OrderLinePoco> Items { get; set; }
    }


    public class CollectionsModel
    {
        public int NoScriptsToday { get; set; }
        public int NoScripts7Days { get; set; }
        public int NoScripts2Weeks { get; set; }
        public int NoScriptsMonth { get; set; }
        public int NoScriptsOlder { get; set; }

        public List<ScriptCollection> PracticesWithScriptToCollect { get; set; } 
    }
}
