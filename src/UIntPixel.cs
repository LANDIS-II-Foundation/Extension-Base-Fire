//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.BaseFire
{
    public class UIntPixel : Pixel
    {
        public Band<uint> MapCode  = "The numeric code for each raster cell";

        public UIntPixel()
        {
            SetBands(MapCode);
        }
    }
}
