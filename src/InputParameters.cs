//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;
using System.Collections.Generic;
using Landis.Core;


namespace Landis.Extension.OriginalFire
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep
        {
            get;set;
        }

        Landis.Library.Parameters.Species.AuxParm<int> FireTolerance { get; }
        string InitialFireRegions { get; set; }

        //---------------------------------------------------------------------
        /// <summary>
        /// Definitions of Fire damages.
        /// </summary>
        List<IDamageTable> FireDamages
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        string MapNamesTemplate
        {
            get;set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of log file.
        /// </summary>
        string FireEventLogFileName
        {
            get;set;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Name of Summary log file.
        /// </summary>
        string FireSummaryLogFileName
        {
            get;set;
        }
        
        List<IDynamicFireRegion> DynamicFireRegions
        {
            get;
        }

    }
}


namespace Landis.Extension.OriginalFire
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        private ISpeciesDataset speciesDataset;
        private string mapNamesTemplate;
        private string logFileName;
        private string summaryLogFileName;

        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                timestep = value;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<int> FireTolerance { get; set; }

        //---------------------------------------------------------------------
        /// <summary>
        /// Definitions of Fire severities.
        /// </summary>
        public List<IDamageTable> FireDamages { get; }
        public string InitialFireRegions { get; set; }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        public string MapNamesTemplate
        {
            get {
                return mapNamesTemplate;
            }
            set {
                if (value != null) {
                    MapNames.CheckTemplateVars(value);
                }
                mapNamesTemplate = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of log file.
        /// </summary>
        public string FireEventLogFileName
        {
            get {
                return logFileName;
            }
            set {
                if (value != null) 
                {
                    // FIXME: check for null or empty path (value.Actual);
                    logFileName = value;
                }
            }
        }

        /// <summary>
        /// Name of log file.
        /// </summary>
        public string FireSummaryLogFileName
        {
            get {
                return summaryLogFileName;
            }
            set {
                if (value != null) {
                    // FIXME: check for null or empty path (value.Actual);
                    summaryLogFileName = value;
                }
            }
        }
        //---------------------------------------------------------------------

        public List<IDynamicFireRegion> DynamicFireRegions { get; }
        //---------------------------------------------------------------------

        public InputParameters(ISpeciesDataset speciesDataset)
        {
            this.speciesDataset = speciesDataset;
            DynamicFireRegions = new List<IDynamicFireRegion>(0);
            FireDamages = new List<IDamageTable>(0);
            FireTolerance = new Landis.Library.Parameters.Species.AuxParm<int>(speciesDataset);
        }
    }
}
