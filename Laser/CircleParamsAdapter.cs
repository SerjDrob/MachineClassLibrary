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
            //formula for calculating parameters
            var innerOffset = 0.001;
            var outerOffset = 0.001;
            var step = 0.001;

            var result = new double[] {innerOffset, outerOffset, step};
            return result;
        }
    }
}
