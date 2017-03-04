//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.AgeOnlyCohorts;
using Landis.Core;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.IO;
using System;

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
        private StreamWriter log;
        private StreamWriter summaryLog;
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

            summaryFireRegionEventCount = new int[FireRegions.Dataset.Count];

            Event.Initialize(parameters.FireDamages);

            PlugIn.ModelCore.UI.WriteLine("   Opening Fire log file \"{0}\" ...", parameters.LogFileName);
            log = Landis.Data.CreateTextFile(parameters.LogFileName);
            log.AutoFlush = true;
            log.Write("Time,InitialSiteRow,InitialSiteColumn,SitesChecked,CohortsKilled,MeanSeverity,");
            foreach (IFireRegion ecoregion in FireRegions.Dataset)
                log.Write("{0},", ecoregion.Name);
            log.Write("TotalBurnedSites");
            log.WriteLine("");

            summaryLog = Landis.Data.CreateTextFile(parameters.SummaryLogFileName);
            summaryLog.AutoFlush = true;
            summaryLog.Write("Time,TotalSitesBurned,TotalNumberEvents");
            foreach (IFireRegion ecoregion in FireRegions.Dataset)
                summaryLog.Write(",{0}", ecoregion.Name);
            summaryLog.WriteLine("");


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
                log.Write("{0},{1},{2},{3},{4},{5:0.0}",
                          currentTime,
                          FireEvent.StartLocation.Row,
                          FireEvent.StartLocation.Column,
                          FireEvent.NumSiteChecked,
                          FireEvent.CohortsKilled,
                          FireEvent.Severity);

                foreach (IFireRegion fireregion in FireRegions.Dataset)
                {
                    log.Write(",{0}", FireEvent.SitesInEvent[fireregion.Index]);
                    totalSitesInEvent += FireEvent.SitesInEvent[fireregion.Index];
                    summaryFireRegionEventCount[fireregion.Index] += FireEvent.SitesInEvent[fireregion.Index];
                }
                summaryTotalSites += totalSitesInEvent;
                log.Write(", {0}", totalSitesInEvent);
                log.WriteLine("");
            }
        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            //int totalSitesInEvent = 0;
            summaryLog.Write("{0},{1},{2}", currentTime, summaryTotalSites, summaryEventCount);
            foreach (IFireRegion ecoregion in FireRegions.Dataset)
            {
                summaryLog.Write(",{0}", summaryFireRegionEventCount[ecoregion.Index]);
            }
            summaryLog.WriteLine("");
        }
    }
}
