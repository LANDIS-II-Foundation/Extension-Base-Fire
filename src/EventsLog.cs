using Landis.Library.Metadata;

namespace Landis.Extension.BaseFire
{
    public class EventsLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Time")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Initial Site Row")]
        public int Row { set; get; }

        [DataFieldAttribute(Desc = "Initial Site Column")]
        public int Column { set; get; }

        [DataFieldAttribute(Desc = "Sites Checked")]
        public int SitesChecked { set; get; }

        [DataFieldAttribute(Desc = "Cohorts Killed")]
        public int CohortsKilled { set; get; }

        [DataFieldAttribute(Desc = "Mean Severity")]
        public double Severity { set; get; }

        [DataFieldAttribute(Desc = "Sites in Fire Event by Ecoregion", ColumnList = true)]
        public double[] SitesEvent { set; get; }

        [DataFieldAttribute(Desc = "Total Burned Sites")]
        public int BurnedSites { set; get; }
    }
}
