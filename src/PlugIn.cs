//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.AgeOnlyCohorts;
using Landis.Core;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.IO;
using Landis.Library.Metadata;
using System;
using System.Diagnostics;

namespace Landis.Extension.BaseFire
{
    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : ExtensionMain 
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:fire");
        public static readonly string ExtensionName = "Base Fire";

        private string mapNameTemplate;
        public static MetadataTable<SummaryLog> summaryLog;
        public static MetadataTable<EventsLog> eventLog;
        private int[] summaryFireRegionEventCount;
        private int summaryTotalSites;
        private int summaryEventCount;
        private List<IDynamicFireRegion> dynamicEcos;
        private IInputParameters parameters;
        private static ICore modelCore;
        
        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            SiteVars.Initialize();
            InputParameterParser parser = new InputParameterParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Timestep = parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;
            dynamicEcos = parameters.DynamicFireRegions;
            string logFileName = parameters.LogFileName;
            string summaryLogFileName = parameters.SummaryLogFileName;

            summaryFireRegionEventCount = new int[FireRegions.Dataset.Count];
            Event.Initialize(parameters.FireDamages);

            modelCore.UI.WriteLine("   Opening and Initializing Fire log files \"{0}\" and \"{1}\"...", parameters.LogFileName, parameters.SummaryLogFileName);
            
            List<string> colnames = new List<string>();
            foreach (IFireRegion fireregion in FireRegions.Dataset)
            {
                colnames.Add(fireregion.Name);
            }
            ExtensionMetadata.ColumnNames = colnames;
            MetadataHandler.InitializeMetadata(Timestep, mapNameTemplate, logFileName, summaryLogFileName);
            
            
            //if (isDebugEnabled)
               // modelCore.UI.WriteLine("Initialization done");
        }

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            PlugIn.ModelCore.UI.WriteLine("   Processing landscape for Fire events ...");

            SiteVars.InitializeDisturbances(Timestep);
            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;
            SiteVars.Disturbed.ActiveSiteValues = false;

            // Update the FireRegions Map as necessary:
            foreach(IDynamicFireRegion dyneco in dynamicEcos)
            {
                 if(dyneco.Year == PlugIn.modelCore.CurrentTime)
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

            foreach (ActiveSite site in PlugIn.modelCore.Landscape) {

                Event FireEvent = Event.Initiate(site, PlugIn.modelCore.CurrentTime, Timestep);
                if (FireEvent != null) {
                    LogEvent(PlugIn.modelCore.CurrentTime, FireEvent);
                    summaryEventCount++;
                }
            }

            //  Write Fire severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, PlugIn.modelCore.CurrentTime);
            using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, modelCore.Landscape.Dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach (Site site in modelCore.Landscape.AllSites) {
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

            WriteSummaryLog(PlugIn.modelCore.CurrentTime);

        }

        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime,
                              Event FireEvent)
        {
            int totalSitesInEvent = 0;
            if (FireEvent.Severity > 0)
            {
                eventLog.Clear();
                EventsLog el = new EventsLog();
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
                el.SitesEvent = new double[fireSites.Length];
                i = 0;
                while(i < fireSites.Length)
                {
                    el.SitesEvent[i] = fireSites[i];
                    ++i;
                }

                summaryTotalSites += totalSitesInEvent;
                el.BurnedSites = totalSitesInEvent;
                eventLog.AddObject(el);
                eventLog.WriteToFile();
            }
        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            summaryLog.Clear();
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

            summaryLog.AddObject(sl);
            summaryLog.WriteToFile();
        }
    }
}
