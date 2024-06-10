//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using System.Collections.Generic;
using Landis.Library.Metadata;

namespace Landis.Extension.OriginalFire
{
    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : ExtensionMain 
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:fire");
        public static readonly string ExtensionName = "Original Fire";

        private string mapNameTemplate;
        public static MetadataTable<SummaryLog> FireSummaryLog;
        public static MetadataTable<FireEventsLog> FireEventLog;
        private int[] summaryFireRegionEventCount;
        private int summaryTotalSites;
        private int summaryEventCount;
        private List<IDynamicFireRegion> dynamicEcos;
        private IInputParameters parameters;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore { get; private set; }
        public override void AddCohortData()
        {
            return;
        }


        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            ModelCore = mCore;
            InputParameterParser parser = new InputParameterParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing Original Fire ...");
            Timestep = parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;
            dynamicEcos = parameters.DynamicFireRegions;

            summaryFireRegionEventCount = new int[FireRegions.Dataset.Count];
            FireEvent.Initialize(parameters.FireDamages);

            SpeciesData.Initialize(parameters);

            List<string> colnames = new List<string>();
            foreach (IFireRegion fireregion in FireRegions.Dataset)
            {
                colnames.Add(fireregion.Name);
            }
            SiteVars.Initialize();
            FireRegions.ReadMap(parameters.InitialFireRegions);

            ModelCore.UI.WriteLine("   Opening and Initializing Fire log files \"{0}\" and \"{1}\"...", parameters.FireEventLogFileName, parameters.FireSummaryLogFileName);
            ExtensionMetadata.ColumnNames = colnames;
            MetadataHandler.InitializeMetadata(Timestep, mapNameTemplate, parameters.FireEventLogFileName, parameters.FireSummaryLogFileName);
        }

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            PlugIn.ModelCore.UI.WriteLine("   Processing landscape for Fire events ...");

            //SiteVars.InitializeDisturbances(Timestep);
            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;
            SiteVars.Disturbed.ActiveSiteValues = false;

            // Update the FireRegions Map as necessary:
            foreach(IDynamicFireRegion dyneco in dynamicEcos)
            {
                 if(dyneco.Year == PlugIn.ModelCore.CurrentTime)
                 {
                     PlugIn.ModelCore.UI.WriteLine("   Reading in new Fire Regions Map {0}.", dyneco.MapName);
                    FireRegions.ReadMap(dyneco.MapName);
                 }
            }

            foreach (IFireRegion fireregion in FireRegions.Dataset)
            {
                summaryFireRegionEventCount[fireregion.Index] = 0;
            }

            summaryTotalSites = 0;
            summaryEventCount = 0;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {

                FireEvent FireEvent = FireEvent.Initiate(site, PlugIn.ModelCore.CurrentTime, Timestep);
                if (FireEvent != null) {
                    LogEvent(PlugIn.ModelCore.CurrentTime, FireEvent);
                    summaryEventCount++;
                }
            }

            //  Write Fire severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, PlugIn.ModelCore.CurrentTime);
            using (IOutputRaster<BytePixel> outputRaster = ModelCore.CreateRaster<BytePixel>(path, ModelCore.Landscape.Dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach (Site site in ModelCore.Landscape.AllSites) {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte)(SiteVars.Severity[site] + 1);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            WriteSummaryLog(PlugIn.ModelCore.CurrentTime);

        }

        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime,
                              FireEvent FireEvent)
        {
            int totalSitesInEvent = 0;
            if (FireEvent.Severity > 0)
            {
                FireEventLog.Clear();
                FireEventsLog el = new FireEventsLog();
                el.Time = currentTime;
                el.Row = FireEvent.StartLocation.Row;
                el.Column = FireEvent.StartLocation.Column;
                el.SitesChecked = FireEvent.NumSiteChecked;
                el.CohortsKilled = FireEvent.CohortsKilled;
                el.Severity = FireEvent.Severity;
                int[] fireSites = new int[FireRegions.Dataset.Count];
                int i = 0;

                foreach (IFireRegion fireregion in FireRegions.Dataset)
                {
                    fireSites[i] = FireEvent.SitesInEvent[fireregion.Index];
                    i = i + 1;
                    totalSitesInEvent += FireEvent.SitesInEvent[fireregion.Index];
                    summaryFireRegionEventCount[fireregion.Index] += FireEvent.SitesInEvent[fireregion.Index];
                }
                el.SitesEventByEcoregion = new double[fireSites.Length];
                i = 0;
                while(i < fireSites.Length)
                {
                    el.SitesEventByEcoregion[i] = fireSites[i];
                    ++i;
                }

                summaryTotalSites += totalSitesInEvent;
                el.BurnedSites = totalSitesInEvent;
                FireEventLog.AddObject(el);
                FireEventLog.WriteToFile();
            }
        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            FireSummaryLog.Clear();
            SummaryLog sl = new SummaryLog();
            sl.Time = currentTime;
            sl.TotalSitesBurned = summaryTotalSites;
            sl.NumberFires = summaryEventCount;

            int[] summaryFireCount = new int[FireRegions.Dataset.Count];
            foreach (IFireRegion ecoregion in FireRegions.Dataset)
            {
                summaryFireCount[ecoregion.Index] = summaryFireRegionEventCount[ecoregion.Index];
            }

            sl.EcoCounts_ = new double[summaryFireCount.Length];
            for(int i = 0; i < sl.EcoCounts_.Length; i++)
            {
                sl.EcoCounts_[i] = summaryFireCount[i];
            }

            FireSummaryLog.AddObject(sl);
            FireSummaryLog.WriteToFile();
        }
    }
}
