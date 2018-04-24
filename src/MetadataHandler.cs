using System;
using System.Collections.Generic;
using System.Linq;
//using System.Data;
using System.Text;
using Landis.Library.Metadata;
using Landis.Core;
using Landis.Utilities;
using System.IO;
using Flel = Landis.Utilities;

namespace Landis.Extension.BaseFire
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension { get; set; }

        public static void InitializeMetadata(int Timestep, string MapFileName, string eventLogName, string summaryLogName)
        {
            
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(PlugIn.ModelCore)
            {
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, //change this to PlugIn.TimeStep for other extensions
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------
            CreateDirectory(eventLogName);
            CreateDirectory(summaryLogName);

            PlugIn.eventLog = new MetadataTable<EventsLog>(eventLogName);
            PlugIn.summaryLog = new MetadataTable<SummaryLog>(summaryLogName);

            //PlugIn.eventLog = new MetadataTable<EventsLog>("Fire-event-log.csv");
            //PlugIn.summaryLog = new MetadataTable<SummaryLog>("Fire-summary-log.csv");

            PlugIn.ModelCore.UI.WriteLine("   Generating event table...");

            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "FireEventsLog",
                FilePath = PlugIn.eventLog.FilePath,
                Visualize = false,
            };

            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);

            PlugIn.ModelCore.UI.WriteLine("   Generating summary table...");
            OutputMetadata tblOut_summary = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "FireSummaryLog",
                FilePath = PlugIn.summaryLog.FilePath,
                Visualize = true,
            };

            tblOut_summary.RetriveFields(typeof(SummaryLog));
            Extension.OutputMetadatas.Add(tblOut_summary);

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            //OutputMetadata mapOut_BiomassRemoved = new OutputMetadata()
            //{
            //    Type = OutputType.Map,
            //    Name = "biomass removed",
            //    FilePath = @HarvestMapName,
            //    Map_DataType = MapDataType.Continuous,
            //    Map_Unit = FieldUnits.Mg_ha,
            //    Visualize = true,
            //};
            //Extension.OutputMetadatas.Add(mapOut_BiomassRemoved);
            

            OutputMetadata mapOut_FireSeverity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Fire_Severity",
                FilePath = MapNames.ReplaceTemplateVars(MapFileName, Timestep),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
                //Map_Unit = "categorical",
            };
            Extension.OutputMetadatas.Add(mapOut_FireSeverity);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);
        }
        public static void CreateDirectory(string path)
        {
            //Require.ArgumentNotNull(path);
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");
            //throw new Exception(path);
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Flel.Directory.EnsureExists(dir);
            }

            //return new StreamWriter(path);
            return;
        }
    }
}
