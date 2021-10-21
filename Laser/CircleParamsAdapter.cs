namespace MachineClassLibrary.Laser
{
    public class CircleParamsAdapter : IParamsAdapting
    {
        private readonly CirclePierceParams circlePierceParams;

        public CircleParamsAdapter(CirclePierceParams circlePierceParams)
        {
            this.circlePierceParams = circlePierceParams;
        }

        public double[] Adapt()
        {
            var result = new double[3];
            return result;
        }
    }
}
