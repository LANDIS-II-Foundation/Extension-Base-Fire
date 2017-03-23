using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.BaseFire
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Time")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Total Sites")]
        public int TotalSites { set; get; }

        [DataFieldAttribute(Desc = "Event Count")]
        public int NumEvents { set; get; }

        [DataFieldAttribute(Desc = "Fire Region Event Count by Ecoregion", ColumnList = true)]
        public int[] EcoCounts_ { set; get; }
    }
}
