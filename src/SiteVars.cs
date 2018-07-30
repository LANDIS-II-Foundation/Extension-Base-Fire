//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.AgeOnlyCohorts;


namespace Landis.Extension.BaseFire
{
    public static class SiteVars
    {
        private static ISiteVar<IFireRegion> ecoregions;
        private static ISiteVar<Event> eventVar;
        private static ISiteVar<int> timeOfLastEvent;
        private static ISiteVar<int> timeOfLastWind;
        private static ISiteVar<byte> severity;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<ISiteCohorts> cohorts;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            ecoregions     = PlugIn.ModelCore.Landscape.NewSiteVar<IFireRegion>();
            eventVar        = PlugIn.ModelCore.Landscape.NewSiteVar<Event>(InactiveSiteMode.DistinctValues);
            timeOfLastEvent = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            severity = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts");

            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Severity, "Fire.Severity");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastFire, "Fire.TimeOfLastEvent");

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                ushort maxAge = GetMaxAge(site);
                //PlugIn.ModelCore.Log.WriteLine("maxAge = {0}.", maxAge);

                timeOfLastEvent[site] = PlugIn.ModelCore.StartTime - maxAge;
            }
        }

        public static void InitializeDisturbances(int timestep)
        {
            timeOfLastWind = PlugIn.ModelCore.GetSiteVar<int>("Wind.TimeOfLastEvent");
        }

        //---------------------------------------------------------------------

        public static ISiteVar<IFireRegion> FireRegion
        {
            get {
                return ecoregions;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<Event> Event
        {
            get {
                return eventVar;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> TimeOfLastFire
        {
            get {
                return timeOfLastEvent;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> TimeOfLastWind
        {
            get {
                return timeOfLastWind;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<byte> Severity
        {
            get {
                return severity;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<bool> Disturbed
        {
            get {
                return disturbed;
            }
        }

        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }
        public static ushort GetMaxAge(ActiveSite site)
        {
            if (SiteVars.Cohorts[site] == null)
            {
                PlugIn.ModelCore.UI.WriteLine("Cohort are null.  Why?");
                return 0;
            }
            ushort max = 0;

            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                        if (cohort.Age > max)
                            max = cohort.Age;
                }
            }
            return max;
        }
    }
}
