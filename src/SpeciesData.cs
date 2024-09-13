//  Author: Robert Scheller, Melissa Lucash


namespace Landis.Extension.OriginalFire
{
    public class SpeciesData 
    {
        public static Landis.Library.Parameters.Species.AuxParm<int> FireTolerance;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            FireTolerance          = parameters.FireTolerance;
        }
    }
}
