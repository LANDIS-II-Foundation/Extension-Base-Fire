using System;
using Landis.Library.Metadata;
using System.IO;

namespace Landis.Extension.OriginalFire
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata FireExtension { get; set; }

        public static void InitializeFireMetadata(int Timestep, string MapFileName, string eventLogName, string summaryLogName)
        {
            
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            FireExtension = new ExtensionMetadata(PlugIn.ModelCore)
            {
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, 
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------
            CreateDirectory(eventLogName);
            CreateDirectory(summaryLogName);

            PlugIn.FireEventLog = new MetadataTable<FireEventsLog>(eventLogName);
            PlugIn.FireSummaryLog = new MetadataTable<SummaryLog>(summaryLogName);

            PlugIn.ModelCore.UI.WriteLine("   Generating event table...");

            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "FireEventsLog",
                FilePath = PlugIn.FireEventLog.FilePath,
                Visualize = false,
            };

            tblOut_events.RetriveFields(typeof(FireEventsLog));
            FireExtension.OutputMetadatas.Add(tblOut_events);

            PlugIn.ModelCore.UI.WriteLine("   Generating summary table...");
            OutputMetadata tblOut_summary = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "FireSummaryLog",
                FilePath = PlugIn.FireSummaryLog.FilePath,
                Visualize = true,
            };

            tblOut_summary.RetriveFields(typeof(SummaryLog));
            FireExtension.OutputMetadatas.Add(tblOut_summary);

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            OutputMetadata mapOut_FireSeverity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Fire_Severity",
                FilePath = MapNames.ReplaceTemplateVars(MapFileName, Timestep),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
            };
            FireExtension.OutputMetadatas.Add(mapOut_FireSeverity);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(FireExtension);
            mp.WriteMetadataToXMLFile("Metadata", FireExtension.Name, FireExtension.Name);
        }
        private static void CreateDirectory(string path)
        {
            path = path.Trim(null);
            
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");
            
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Landis.Utilities.Directory.EnsureExists(dir);
            }

            return;
        }
    }
}
