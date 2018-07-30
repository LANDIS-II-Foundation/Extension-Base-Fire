//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
//using Landis.SpatialModeling.CoreServices;
using Landis.Utilities;
using System.Collections.Generic;

namespace Landis.Extension.BaseFire
{
    /// <summary>
    /// The parameters for an ecoregion.
    /// </summary>
    public interface IFireRegion
    {
        string Name {get;set;}
        ushort MapCode {get;set;}
        double MaxSize {get;set;}
        double MeanSize {get;set;}
        double MinSize {get;set;}
        double IgnitionProbability {get;set;}
        int FireSpreadAge {get;set;}
        IFuelCurve FuelCurve {get;set;}
        IWindCurve WindCurve {get;set;}
        int Index {get; set;}

    }
}

namespace Landis.Extension.BaseFire
{
    public class FireRegion
        : IFireRegion
    {
        private string name;
        private ushort mapCode;
        private double meanSize;
        private double maxSize;
        private double minSize;
        private double ignitionProbability;
        private int fireSpreadAge;
        private IFuelCurve fuelCurve;
        private IWindCurve windCurve;
        private int index;

        public int Index
        {
            get {
                return index;
            }
            set {
                index = value;
            }
        }

        //---------------------------------------------------------------------

        public string Name
        {
            get {
                return name;
            }
            set {
                //if (value != null) {
                    if (value.Trim() == "")
                        throw new InputValueException(value, "Missing name");
                //}
                name = value;
            }
        }


        //---------------------------------------------------------------------

        public ushort MapCode
        {
            get {
                return mapCode;
            }
            set {
                mapCode = value;
            }
        }


        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MeanSize
        {
            get {
                return meanSize;
            }
            set {
                //if (value != null) {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                //}
                meanSize = value;
            }
        }

        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MaxSize
        {
            get {
                return maxSize;
            }
            set {
                //if (value != null) {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                //}
                maxSize = value;
            }
        }
        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MinSize
        {
            get {
                return minSize;
            }
            set {
                //if (value != null) {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                //}
                minSize = value;
            }
        }

        /// <summary>
        /// Ignition probability
        /// </summary>
        public double IgnitionProbability
        {
            get {
                return ignitionProbability;
            }
            set {
                //if (value != null) {
                    if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(), "Value must be >= 0 and <= 1.0");
                //}
                ignitionProbability = value;
            }
        }
        //---------------------------------------------------------------------


        public int FireSpreadAge
        {
            get {
                return fireSpreadAge;
            }
            set {
                //if (value != null) {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                //}
                fireSpreadAge = value;
            }
        }
        //---------------------------------------------------------------------

        public IFuelCurve FuelCurve
        {
            get {
                return fuelCurve;
            }

            set {
                fuelCurve = value;
            }
        }

        //---------------------------------------------------------------------

        public IWindCurve WindCurve
        {
            get {
                return windCurve;
            }

            set {
                windCurve = value;
            }
        }

        //---------------------------------------------------------------------

        public FireRegion(int index)
        {
            this.fuelCurve = new FuelCurve();
            this.windCurve = new WindCurve();
            this.index = index;
        }
        //---------------------------------------------------------------------

    }
}
