using Landis.Library.Metadata;

namespace Landis.Extension.BaseFire
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Time")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Total Sites")]
        public int TotalSitesBurned { set; get; }

        [DataFieldAttribute(Desc = "Event Count")]
        public int NumberFires { set; get; }

        [DataFieldAttribute(Desc = "Fire Region Event Count by Ecoregion", ColumnList = true)]
        public double[] EcoCounts_ { set; get; }
    }
}
