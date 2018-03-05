namespace ParkPlaces.Controls
{
    public class VerticleChangedArg
    {
        public int VerticlesCount { get; }
        public int ShapesCount { get; }

        public VerticleChangedArg(int verticlesCount, int shapesCount)
        {
            VerticlesCount = verticlesCount;
            ShapesCount = shapesCount;
        }
    }
}
