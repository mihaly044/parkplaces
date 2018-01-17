using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.Controls
{
    public class VerticleChangedArg
    {
        public int VerticlesCount { get; private set; }
        public int ShapesCount { get; private set; }

        public VerticleChangedArg(int VerticlesCount, int ShapesCount)
        {
            this.VerticlesCount = VerticlesCount;
            this.ShapesCount = ShapesCount;
        }
    }
}
